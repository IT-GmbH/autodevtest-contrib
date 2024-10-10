using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace etsdevtest.cli;

class HelpCommand : Command
{
    RootCommand mRootCommand;

    public async Task<int> Run(string[] aCommand)
    {
        try
        {
            Command baseCommand = mRootCommand;
            foreach (var cmd in aCommand)
            {
                baseCommand = baseCommand.Subcommands.First(c => c.Name == cmd);
                if (baseCommand == null)
                {
                    throw new ArgumentException("No command named 'z'");
                }
            }

            foreach (var command in baseCommand.Subcommands)
            {
                Console.WriteLine("{0,-15} {1,0}", command.Name, command.Description);
            }
        }
        catch (Exception ex)
        {
            return CommandProcessor.ProcessException(ex);
        }
        return 0;
    }

    public HelpCommand(RootCommand aRootCommand) : base("help", "show this help command")
    {
        mRootCommand = aRootCommand;
        var helpCmd = new Argument<string[]>("command");
        this.Add(helpCmd);
        this.SetHandler(Run, helpCmd);
    }
}