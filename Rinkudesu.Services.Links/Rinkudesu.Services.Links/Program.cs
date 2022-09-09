using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using CommandLine;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rinkudesu.Kafka.Dotnet;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Services.Links;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.DataTransferObjects;
using Rinkudesu.Services.Links.HealthChecks;
using Rinkudesu.Services.Links.HostedServices;
using Rinkudesu.Services.Links.MessageHandlers;
using Rinkudesu.Services.Links.MessageQueues.Messages;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Utilities;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
#pragma warning disable 1591
#pragma warning disable CA1812

// Migrations adding is behaving in a rather bizarre way.
// This line is required to fix some arguments being passed to this program
// and also theres a fatal exception somewhere during it
// However, the migration is generated correctly
#if DEBUG
args = args.Where(a => a != "--applicationName").ToArray();
#endif

var result = Parser.Default.ParseArguments<InputArguments>(args)
    .WithParsed(o => {
        o.SaveAsCurrent();
    });
if (result.Tag == ParserResultType.NotParsed)
{
    return 1;
}
var logConfig = new LoggerConfiguration();
if (!InputArguments.Current.MuteConsoleLog)
{
    logConfig.WriteTo.Console();
}
Log.Logger = logConfig.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureServices(builder.Services);

    builder.Host.UseSerilog((context, services, configuration) => {
        if (!InputArguments.Current.MuteConsoleLog)
        {
            configuration.WriteTo.Console();
        }
        if (InputArguments.Current.FileLogPath != null)
        {
            configuration.WriteTo.File(new RenderedCompactJsonFormatter(),
                InputArguments.Current.FileLogPath);
        }
        InputArguments.Current.GetMinimumLogLevel(configuration);
        configuration.ReadFrom.Services(services);
        configuration.Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
            .Enrich.WithExceptionDetails();
    });
    builder.WebHost
        .UseKestrel((context, serverOptions) => {
            serverOptions.ConfigureHttpsDefaults(https => {
                https.ServerCertificate = new X509Certificate2("cert.pfx");
            });
        });

    var app = builder.Build();
    Configure(app, app.Environment);
    await app.RunAsync();
}
#pragma warning disable CA1031
catch (Exception e)
#pragma warning restore CA1031
{
    Log.Fatal(e, "Application failed to start");
    return 2;
}
finally
{
    Log.CloseAndFlush();
}
return 0;

void ConfigureServices(IServiceCollection services)
{
    SetupKafka(services);
    services.AddScoped<ILinkRepository, LinkRepository>();
    services.AddAutoMapper(typeof(LinkMappingProfile));
    services.AddScoped<ISharedLinkRepository, SharedLinkRepository>();

    services.AddSingleton<IKafkaSubscriber<UserDeletedMessage>, KafkaSubscriber<UserDeletedMessage>>();
    services.AddSingleton<IKafkaSubscriberHandler<UserDeletedMessage>, UserDeletedMessageHandler>();
    services.AddHostedService<UserDeletedQueueListener>();

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

    var requireAuthenticated = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    services.AddControllers(o =>
        {
            o.Filters.Add(new AuthorizeFilter(requireAuthenticated));
        })
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => {
            options.Authority = Environment.GetEnvironmentVariable("RINKUDESU_AUTHORITY");
#if DEBUG
            options.RequireHttpsMetadata = false;
#endif
            options.Audience = "rinkudesu";
        });

    services.AddApiVersioning(o => {
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

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

    services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("Database health check");
}

void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

    app.UseAuthentication();

    app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
    });

    if (InputArguments.Current.ApplyMigrations)
    {
        ApplyMigrations(app);
    }
}

static void ApplyMigrations(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<LinkDbContext>();
    context.Database.Migrate();
}

static void SetupKafka(IServiceCollection serviceCollection)
{
    var kafkaConfig = KafkaConfigurationProvider.ReadFromEnv();
    serviceCollection.AddSingleton(kafkaConfig);
    serviceCollection.AddSingleton<IKafkaProducer, KafkaProducer>();
}
