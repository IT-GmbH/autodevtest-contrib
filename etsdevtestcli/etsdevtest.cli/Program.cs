using System;
using System.Linq;
using System.Threading.Tasks;
using etsdevtest.cli;
using Itgmbh.AutoDevTest;

internal class Program
{

    static async Task Main(string[] args)
    {
        var ets6Factory = new Ets6Factory();
        var config = new Config(ets6Factory);
        using var appInstance = new AppInstance(ets6Factory, config);
        var mProcessor = new CliCommandProcessor(
            ets6Factory,
            config,
            appInstance
        );

        await mProcessor.Process("\"" + String.Join("\" \"", args) + "\"");
    }
}