using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading.Tasks;

namespace etsdevtest.cli;

public class CommandProcessor
{
    public const int kNoError = 0;
    public const int kUnknownError = 1;
    public const int kNotSupportedError = 15;
    public const int kDownloadFailed = 16;
    public const int kFailedToStartCommand = 17;
    public const int kIgnoreDone = 100;

    public static int ProcessException(Exception ex)
    {
        Console.WriteLine($"Error {ex.Message}");
        return kIgnoreDone;
    }

    public static async Task<int> NotSupported()
    {
        return await Task.FromResult(CommandProcessor.kNotSupportedError);
    }

    protected RootCommand mRootCommand = new RootCommand("ETS6 Dev Tool");
    CustomConsole mConsole;
    AppInstance mAppInstance;

    public CommandProcessor(IEts6Factory aEts6Factory, IConfig aConfig, AppInstance aApp, CustomConsole aConsole = null)
    {
        mConsole = aConsole;
        mAppInstance = aApp;

        mRootCommand.AddCommand(new ConfigCommand(aConfig));
        mRootCommand.AddCommand(new StartCommand(mAppInstance, aConfig));
        mRootCommand.AddCommand(new ProjectCommand(aEts6Factory, aConfig, aApp));
        mRootCommand.AddCommand(new DeviceCommand(aApp));
        mRootCommand.AddCommand(new VersionCommand());
    }

    void WriteError(string aErrorMessage)
    {
        if (mConsole != null)
        {
            mConsole.Error.WriteLine($"Error {aErrorMessage}");
        }
        else
        {
            Console.Error.WriteLine($"Error {aErrorMessage}");
        }
    }

    void Done()
    {
        if (mConsole != null)
        {
            mConsole.Out.WriteLine("Done");
        }
        else
        {
            Console.Out.WriteLine("Done");
        }
    }

    public string[] ParseArgs(string aArgs)
    {
        return [];
    }

    public async Task<int> Process(string aArgs)
    {
        int error = 0;
        try
        {
            error = await mRootCommand.InvokeAsync(aArgs, mConsole);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            WriteError(ex.Message);
        }

        var result = mRootCommand.Parse(aArgs);

        if (result.Errors.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var ex in result.Errors)
            {
                sb.Append(" ");
                sb.Append(ex.Message);
            }
            sb.Remove(0, 1);
            WriteError(sb.ToString());
            return kUnknownError;
        }

        switch (error)
        {
            case 0:
                Done();
                break;
            case kIgnoreDone:
                break;
            case kNotSupportedError:
                WriteError("Command not supported");
                break;
            case kDownloadFailed:
                WriteError("Download Failed.");
                break;
            case kFailedToStartCommand:
                WriteError("Failed to start the command.");
                break;
            default:
                WriteError("Command Failed.");
                break;
        }
        return error;
    }
}