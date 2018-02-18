using System;
using System.Threading.Tasks;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ERNI.PhotoDatabase.DataAccess;
using ERNI.PhotoDatabase.Server.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ERNI.PhotoDatabase.DataAccess.DomainModel;
using System.Collections.Generic;

namespace ERNI.PhotoDatabase.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(
                auth =>
                {
                    auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }
            )
            .AddCookie(opt =>
            {
                opt.LoginPath = "/";                
            })
            .AddOpenIdConnect(options =>
            {
                options.ClientId = Configuration["Authentication:AzureAd:ClientId"];
                options.Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"];
                options.CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"];
                options.Events.OnTokenValidated = async context =>
                {

                    var db = context.HttpContext.RequestServices.GetRequiredService<DatabaseContext>();

                    var sub = context.Principal.Claims.Single(c => c.Type == "sub").Value;

                    var user = await db.Users.SingleOrDefaultAsync(_ => _.UniqueIdentifier == sub);

                    if (user == null)
                    {

                        user = new User
                        {
                            UniqueIdentifier = sub
                        };
                        db.Users.Add(user);
                        db.SaveChanges();
                    }

                    var claims = new List<System.Security.Claims.Claim>();

                    if (user.CanUpload)
                    {
                        claims.Add(new System.Security.Claims.Claim("role", "uploader"));
                    }

                    if (user.IsAdmin)
                    {
                        claims.Add(new System.Security.Claims.Claim("role", "admin"));
                    }

                    context.Principal.AddIdentity(new System.Security.Claims.ClaimsIdentity(claims, null, null, "role"));
                };
            });

            services.Configure<ImageSizesSettings>(Configuration.GetSection("ImageSizes"));

            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Database")));

            services.AddMvc();

            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

                context.Database.EnsureCreated();

                context.Database.Migrate();
            }

            ConfigureImageStore(app.ApplicationServices).Wait();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }

        private async Task ConfigureImageStore(IServiceProvider serviceProvider)
        {
            var imageStoreConfiguration = serviceProvider.GetRequiredService<DataAccess.Images.ImageStoreConfiguration>();
            imageStoreConfiguration.ConnectionString = Configuration.GetConnectionString("BlobStorage");
            imageStoreConfiguration.ContainerName = Configuration.GetValue<string>("BlobContainerName");

            using (var scope = serviceProvider.CreateScope())
            {
                var imageStore = scope.ServiceProvider.GetRequiredService<DataAccess.Images.ImageStore>();
                await imageStore.InitializeAsync(CancellationToken.None);
            }
        }
    }
}
