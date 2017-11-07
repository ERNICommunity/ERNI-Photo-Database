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
            // if (env.IsDevelopment())
            // {
                app.UseDeveloperExceptionPage();
            // }

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
                if (env.IsDevelopment())
                {
                    context.Database.EnsureDeleted();
                }

                context.Database.EnsureCreated();
            }

            this.ConfigureImageStore(app.ApplicationServices).Wait();

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
