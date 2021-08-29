using System;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
#pragma warning disable 1591

namespace Rinkudesu.Services.Links
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static int Main(string[] args)
        {
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
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application failed to start");
                return 2;
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => {
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
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}