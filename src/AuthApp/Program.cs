using System.Reflection;
using System.Threading.Tasks;

using AuthApp.Internal;

using McMaster.Extensions.CommandLineUtils;

using Console = Colorful.Console;

namespace AuthApp
{
    [Command(Name = Constants.CLIToolName, Description = "cli tool to help with Salesforce development by providing with Refresh token generation.")]
    [Subcommand(typeof(TokenGeneratorCommand))]
    [HelpOption("-?")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        private static Task<int> Main(string[] args)
        {
            return CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private static string GetVersion()
        {
            return typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            Console.WriteAscii(Constants.CLIToolName, Colorful.FigletFont.Default);

            console.WriteLine();
            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}
