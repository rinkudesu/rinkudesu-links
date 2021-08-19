using CommandLine;

namespace Rinkudesu.Services.Links
{
    public class InputArguments
    {
        public static InputArguments Current { get; private set; } = null!;// the value is always set as the first operation of the program
        [Option(longName: "applyMigrations", Required = false, HelpText = "Automatically creates the database and applies any missing migrations on startup")]
        public bool ApplyMigrations { get; set; }

        public void SaveAsCurrent()
        {
            Current = this;
        }
    }
}