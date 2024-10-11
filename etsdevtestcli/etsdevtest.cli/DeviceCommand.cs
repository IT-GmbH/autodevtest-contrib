
using System;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
        AppInstance mAppInstance;

        void ProcessState(ushort aIa, OnlineOperationState aState, string aMessage)
        {
            if (aState == OnlineOperationState.Running)
            {
                throw new Exception($"{aMessage}, timed out");
            }
            if (aState != OnlineOperationState.Finished)
            {
                var error = mAppInstance.Get().GetOnlineOperationErrorMessage(aIa);
                throw new Exception($"{aMessage}, failed with {aState} - {error}");
            }
        }

        public DownloadCommand(AppInstance appInstance) : base("download", "download device by serialnumber")
        {
            mAppInstance = appInstance;
            var srcSia = new IndividualAddressArgument("Device identified by sia x.x.x");
            this.Add(srcSia);
            var snArg = new SerialnumberArgument(aDefault: true);
            Add(snArg);
            var pwArg = new Argument<string>("pw", () => null, "password to use, if not provided will not configure password");
            Add(pwArg);
            this.SetHandler((src, sn, pw) =>
            {
                var error = 0;
                try
                {

                    OnlineOperationState state;

                    if (pw != null)
                    {
                        appInstance.SetDeviceCertificate(src, sn, pw);
                    }

                    if (sn.Value.Length > 0)
                    {
                        Console.WriteLine($"Resolve serialnumber '{sn.String}'");
                        appInstance.Get().StartLoadNetworkConfiguration(src, sn.Value);
                        state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, 100000);
                        Console.WriteLine($"Serialnumber resolve state '{state}'");
                        ProcessState(src, state, "Serialnumber resolving failed");
                    }
                    else
                    {
                        Console.WriteLine($"Download file by programming mode");
                    }

                    appInstance.Get().StartOnlineOperation(src, OnlineOperationType.DownloadFull);

                    state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, 100000);
                    Console.WriteLine($"Download state '{state}'");
                    ProcessState(src, state, "Download failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    error = CommandProcessor.kDownloadFailed;
                }
                return Task.FromResult(error);
            }, srcSia, snArg, pwArg);
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
            this.SetHandler(appInstance.SetDeviceCertificate, srcSia, snArg, pwArg);
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