using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.DataTransferObjects;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Utilities;
using Serilog;
#pragma warning disable 1591

namespace Rinkudesu.Services.Links
{
    [ExcludeFromCodeCoverage]
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
            services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Link management API",
                    Description = "API to manage link objects"
                });
                var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = AppContext.BaseDirectory;
                c.IncludeXmlComments(Path.Combine(xmlPath, xmlName));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Links API V1");
            });

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