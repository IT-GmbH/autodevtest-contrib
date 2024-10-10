
using System;
using System.CommandLine;
using System.Text;
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

    class ChangeAddress : Command
    {
        public ChangeAddress(AppInstance appInstance) : base("change-address", "change an ia address with another")
        {
            var srcSia = new IndividualAddressArgument("Device identified by sia x.x.x");
            var dstSia = new Argument<byte>("New line address");
            this.Add(srcSia);
            this.Add(dstSia);
            this.SetHandler((src, dst) =>
            {
                appInstance.Get().ChangeDeviceAddress(src, dst);
            }, srcSia, dstSia);
        }
    }

    class DownloadCommand : Command
    {
        public DownloadCommand(AppInstance appInstance) : base("download", "download device by serialnumber")
        {
            var srcSia = new IndividualAddressArgument("Device identified by sia x.x.x");
            this.Add(srcSia);
            var snArg = new SerialnumberArgument(aDefault: true);
            Add(snArg);
            this.SetHandler((src, sn) =>
            {
                OnlineOperationState state;
                if (sn.Value.Length > 0)
                {
                    Console.WriteLine($"Resolve serialnumber '{sn.String}'");
                    appInstance.Get().StartLoadNetworkConfiguration(src, sn.Value);
                    state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, 100000);
                    Console.WriteLine($"Serialnumber resolve state '{state}'");
                }
                else
                {
                    Console.WriteLine($"Download file by programming mode");
                }

                appInstance.Get().StartOnlineOperation(src, OnlineOperationType.DownloadFull);

                state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, 100000);
                Console.WriteLine($"Download state '{state}'");
            }, srcSia, snArg);
        }
    }

    class SerialnumberCommand : Command
    {
        public SerialnumberCommand(AppInstance appInstance) : base("sn", "set serialnumber of sia")
        {
            var srcSia = new IndividualAddressArgument("Device identified by sia x.x.x");
            Add(srcSia);
            var snArg = new SerialnumberArgument(aDefault: true);
            Add(snArg);
            this.SetHandler((sia, sn) =>
            {
                appInstance.Get().SetDeviceSerialNumber(sia, sn.Value);
            }, srcSia, snArg);
        }
    }

    class CertificateCommand : Command
    {
        public CertificateCommand(AppInstance appInstance) : base("certificate", "configure certificate for device <ia>")
        {
            var srcSia = new IndividualAddressArgument("Device identified by sia x.x.x");
            Add(srcSia);
            var snArg = new SerialnumberArgument();
            Add(snArg);
            var pwArg = new Argument<string>("certificate", "Certificate string (DMC Code or KNX:S:<sn>;P:<pw>)");
            Add(pwArg);
            this.SetHandler((sia, sn, pw) =>
            {
                appInstance.Get().SetDeviceSerialNumber(sia, sn.Value);
                appInstance.Get().AddRawDeviceCertificate($"KNX:S:{sn.String};P:{pw}");
            }, srcSia, snArg, pwArg);
        }
    }

    public DeviceCommand(AppInstance aAppInstance) : base("device", "commands related to manage a device")
    {
        AddCommand(new ParametersCommand(aAppInstance));
        AddCommand(new ChangeAddress(aAppInstance));
        AddCommand(new DownloadCommand(aAppInstance));
        AddCommand(new CertificateCommand(aAppInstance));
        AddCommand(new SerialnumberCommand(aAppInstance));
    }

}