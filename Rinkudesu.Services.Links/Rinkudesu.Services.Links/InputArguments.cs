using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Serilog;
#pragma warning disable 1591

namespace Rinkudesu.Services.Links
{
    [ExcludeFromCodeCoverage]
    public class InputArguments
    {
        public static InputArguments Current { get; private set; } = null!; // the value is always set as the first operation of the program
        [Option(longName: "applyMigrations", Required = false, HelpText = "Automatically creates the database and applies any missing migrations on startup")]
        public bool ApplyMigrations { get; set; }
        [Option(longName: "muteConsoleLog", Required = false, HelpText = "Disables logging to console")]
        public bool MuteConsoleLog { get; set; }
        [Option(shortName: 'l', longName: "logLevel", Required = false, Default = 2, HelpText = "Defines log level: 0 - log everything, 5 - log errors only")]
        public int LogLevel { get; set; }
        [Option(longName: "logPath", Required = false, HelpText = "Optional path to a log file. Logs won't be saved to file if this is ignored.")]
        public string? FileLogPath { get; set; }

        public void SaveAsCurrent()
        {
            Current = this;
        }

        public LoggerConfiguration GetMinimumLogLevel(LoggerConfiguration configuration)
        {
            return LogLevel switch
            {
                0 => configuration.MinimumLevel.Verbose(),
                1 => configuration.MinimumLevel.Debug(),
                2 => configuration.MinimumLevel.Information(),
                3 => configuration.MinimumLevel.Warning(),
                4 => configuration.MinimumLevel.Error(),
                _ => configuration.MinimumLevel.Information()
            };
        }
    }
}