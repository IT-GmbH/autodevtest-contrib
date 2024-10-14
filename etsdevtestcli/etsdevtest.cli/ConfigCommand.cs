using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Drawing;

namespace etsdevtest.cli;

class ConfigCommand : Command
{
    static IConfig.Types GetConfigKey(ArgumentResult aResult)
    {
        return IConfig.Types.ProjectStore;
    }

    static Argument<IConfig.Types> GetKeyArg()
    {
        return new Argument<IConfig.Types>("key", "provide the config key");
    }

    class SetCommand : Command
    {
        public SetCommand(IConfig aConfig)
            : base("set", "set a config value <key> <value>")
        {
            var KeyArg = GetKeyArg();
            var ValueArg = new Argument<string>("value", "The value to set the key to");
            this.Add(KeyArg);
            this.Add(ValueArg);
            this.SetHandler(aConfig.Set, KeyArg, ValueArg);
        }
    }

    class GetCommand : Command
    {
        public GetCommand(IConfig aConfig)
            : base("get", "get a config by key")
        {
            var KeyArg = GetKeyArg();
            this.Add(KeyArg);
            this.SetHandler((IConfig.Types aKey) =>
            {
                Console.WriteLine($"Value: '{aConfig.Get(aKey, "")}'");
            }, KeyArg);
        }
    }

    class ListCommand : Command
    {
        public ListCommand(IConfig aConfig)
            : base("list", "list all available configurations")
        {
            this.SetHandler(() =>
            {
                Console.WriteLine("Config file: {0}", aConfig.GetConfigFile());
                Console.WriteLine("{0,-15} {1,5}", "Key", "Value");
                Console.WriteLine(new string('-', 30));
                foreach (var obj in Enum.GetValues(typeof(IConfig.Types)))
                {
                    Console.WriteLine("{0,-15} '{1,0}'", Enum.GetName(typeof(IConfig.Types), obj), aConfig.Get((IConfig.Types)obj));
                }
            });
        }
    }

    class ClearCommand : Command
    {
        public ClearCommand(IConfig aConfig)
         : base("clear", "clear all stored configuration")
        {
            this.SetHandler(aConfig.Clear);
        }
    }

    public ConfigCommand(IConfig aConfig) : base("config", "Set configuration values")
    {
        AddCommand(new SetCommand(aConfig));
        AddCommand(new GetCommand(aConfig));
        AddCommand(new ListCommand(aConfig));
        AddCommand(new ClearCommand(aConfig));
    }
}