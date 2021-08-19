using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Rinkudesu.Services.Links
{
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

            CreateHostBuilder(args).Build().Run();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}