using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ERNI.PhotoDatabase.DataAccess;
using ERNI.PhotoDatabase.DataAccess.Repository;
using ERNI.PhotoDatabase.DataAccess.UnitOfWork;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetValue<string>("ConnectionString")));

            services.AddScoped<IPhotoRepository, PhotoRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<DataAccess.Images.ImageStoreConfiguration>();
            services.AddScoped<DataAccess.Images.ImageStore>();
            services.AddMvc();
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
                if (env.IsDevelopment())
                {
                    context.Database.EnsureDeleted();
                }

                context.Database.EnsureCreated();
            }

            ConfigureImageStore(app.ApplicationServices).Wait();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }

        private async Task ConfigureImageStore(IServiceProvider serviceProvider)
        {
            var imageStoreConfiguration = serviceProvider.GetRequiredService<DataAccess.Images.ImageStoreConfiguration>();
            var imageStoreSection = Configuration.GetSection("imageStore");
            imageStoreConfiguration.ConnectionString = imageStoreSection.GetValue("connectionString", imageStoreConfiguration.ConnectionString);
            imageStoreConfiguration.ContainerName = imageStoreSection.GetValue("containerName", imageStoreConfiguration.ContainerName);

            using (var scope = serviceProvider.CreateScope())
            {
                var imageStore = scope.ServiceProvider.GetRequiredService<DataAccess.Images.ImageStore>();
                await imageStore.InitializeAsync(CancellationToken.None);
            }
        }
    }
}
