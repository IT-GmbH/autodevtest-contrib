
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace etsdevtest.cli;

/// <summary>
/// Add commands only available in shell interface
/// </summary>
public class ShellCommand : CommandProcessor
{
    bool IsRunning = true;
    bool CancelKeyPressed = false;
    CancellationTokenSource cancellationTokenSource;

    void PressCancelKey(object sender, ConsoleCancelEventArgs args)
    {
        if (cancellationTokenSource != null)
        {
            Task.Run(() => cancellationTokenSource.Cancel());
        }

        if (CancelKeyPressed)
        {
            return;
        }

        CancelKeyPressed = true;
        Console.WriteLine("\nPress CTRL+C again to exit");
        args.Cancel = true;
    }

    private void CancelNotification()
    {
        cancellationTokenSource = null;
    }

    public async Task<int> Run()
    {
        Console.Clear();
        Console.CancelKeyPress += new ConsoleCancelEventHandler(PressCancelKey);

        while (IsRunning)
        {
            Console.Write("> ");
            var cmd = Console.ReadLine();
            if (cmd == string.Empty)
            {
                CancelKeyPressed = false;
            }

            if (!string.IsNullOrWhiteSpace(cmd))
            {
                // configure cancellation token
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.Token.Register(CancelNotification);
                var completionSource = new TaskCompletionSource<object>();
                cancellationTokenSource.Token.Register(() => completionSource.TrySetCanceled());

                var task = Task.Run(async () => await Process(cmd), cancellationTokenSource.Token);

                await Task.WhenAny(task, completionSource.Task);
                cancellationTokenSource = null;
            }
        }
        return kIgnoreDone;
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
        mRootCommand.Add(new HelpCommand(mRootCommand));
    }

}