
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace etsdevtest.cli;

/// <summary>
/// Add commands only available in shell interface
/// </summary>
public class ShellCommand : CommandProcessor
{
    bool IsRunning = true;

    public async Task<int> Run()
    {
        while (IsRunning)
        {
            Console.Write("> ");
            await Process(Console.ReadLine());
        }
        return CommandProcessor.kIgnoreDone;
    }

    void Exit()
    {
        IsRunning = false;
    }

    class ExitCommand : Command
    {
        public ExitCommand(ShellCommand aShellCommand) : base("exit", "exit the interactive shell")
        {
            this.SetHandler(() =>
            {
                aShellCommand.Exit();
            });
        }
    }

    public ShellCommand(IEts6Factory aEtsFactory, IConfig aConfig, AppInstance aApp, CustomConsole aConsole = null) : base(aEtsFactory, aConfig, aApp, aConsole)
    {
        mRootCommand.Add(new ExitCommand(this));
    }

}