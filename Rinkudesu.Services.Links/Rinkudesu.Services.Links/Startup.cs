using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.DataTransferObjects;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links
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
            services.AddScoped<ILinkRepository, LinkRepository>();
            services.AddAutoMapper(typeof(LinkMappingProfile));

            services.AddDbContext<LinkDbContext>(options => {
                options.UseNpgsql(
                    EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader
                        .DbContextVariableName), providerOptions => {
                        providerOptions.EnableRetryOnFailure();
                        providerOptions.MigrationsAssembly("Rinkudesu.Services.Links");
                    });
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            if (InputArguments.Current.ApplyMigrations)
            {
                ApplyMigrations(app);
            }
        }

        private static void ApplyMigrations(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<LinkDbContext>();
            context.Database.Migrate();
        }
    }
}