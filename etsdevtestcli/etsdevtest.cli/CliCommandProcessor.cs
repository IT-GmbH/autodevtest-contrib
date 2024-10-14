using System.CommandLine;
using System.CommandLine.Binding;
using System.Threading.Tasks;

namespace etsdevtest.cli;

/// <summary>
/// Cli processor adding commands only available from command line interface
/// </summary>
public class CliCommandProcessor : CommandProcessor
{
    class ShellBinder : BinderBase<ShellCommand>
    {
        IEts6Factory mEtsFactory;
        IConfig mConfig;
        CustomConsole mConsole;
        AppInstance mAppInstance;

        public ShellBinder(IEts6Factory aEtsFactory, IConfig aConfig, AppInstance aAppInstance, CustomConsole aConsole)
        {
            mEtsFactory = aEtsFactory;
            mConfig = aConfig;
            mConsole = aConsole;
            mAppInstance = aAppInstance;
        }

        protected override ShellCommand GetBoundValue(BindingContext bindingContext) =>
            new ShellCommand(mEtsFactory, mConfig, mAppInstance, mConsole);
    };

    static async Task<int> Run(ShellCommand aCommand)
    {
        return await aCommand.Run();
    }

    public CliCommandProcessor(IEts6Factory aEtsFactory, IConfig aConfig, AppInstance aApp, CustomConsole aConsole = null) : base(
        aEtsFactory, aConfig, aApp, aConsole
    )
    {
        var shellCommand = new Command("shell", "start a shell to interactively work with ets6");
        shellCommand.SetHandler(Run, new ShellBinder(aEtsFactory, aConfig, aApp, aConsole));
        mRootCommand.Add(shellCommand);
    }
}