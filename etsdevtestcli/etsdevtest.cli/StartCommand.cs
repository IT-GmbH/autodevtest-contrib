
using System;
using System.CommandLine;

namespace etsdevtest.cli;

class StartCommand : Command
{
    AppInstance _appInstance;

    public static Argument<string> GetProjectArg(IConfig aConfig)
    {
        return new Argument<string>("project", () =>
        {
            return aConfig.Get(IConfig.Types.DefaultProject, "");
        }, $"Project name, defaults to '{aConfig.Get(IConfig.Types.DefaultProject, "<null>")}'");
    }

    public StartCommand(AppInstance appInstance, IConfig aConfig) : base("start", "Start ets6 with a project")
    {
        _appInstance = appInstance;
        var argProjectName = GetProjectArg(aConfig);
        var optPassword = new Option<string>(["-p", "--password"], () => null, "project password");
        var optImport = new Option<string>(["-i", "--import-from"], () => null, "file to import project from");
        var optTimeoutMs = new Option<int>(["-t", "--timout"], () => 30000, "provide maximum timout in milliseconds");

        Add(argProjectName);
        Add(optPassword);
        Add(optImport);
        Add(optTimeoutMs);
        this.SetHandler(appInstance.Start, argProjectName, optPassword, optImport, optTimeoutMs);
    }
}