
using System;
using System.CommandLine;
using Itgmbh.AutoDevTest;

namespace etsdevtest.cli;

class DeviceCommand : Command
{
    class ParametersCommand : Command
    {
        public ParametersCommand(AppInstance appInstance) : base("parameters", "Get active device parameters")
        {
            var siaArg = new IndividualAddressArgument();
            Add(siaArg);
            this.SetHandler((sia) =>
            {
                Console.WriteLine(Serialization.SerializeParameters(appInstance.Get().GetActiveDeviceParameters(sia)));
            }, siaArg);
        }
    }

    public DeviceCommand(AppInstance aAppInstance) : base("device", "commands related to manage a device")
    {
        AddCommand(new ParametersCommand(aAppInstance));
    }

}