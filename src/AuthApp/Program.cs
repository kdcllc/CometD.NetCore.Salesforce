using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Console = Colorful.Console;

namespace AuthApp
{
    [Command(Name = "salesforce", Description = "cli tool to help with salesforce development.")]
    [Subcommand(typeof(TokenGeneratorCommand))]
    [HelpOption("-?")]
    public class Program
    {
        private static Task<int> Main(string[] args)
        {
            return CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            Console.WriteAscii("Salesforce", Colorful.FigletFont.Default);

            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }
    }
}
