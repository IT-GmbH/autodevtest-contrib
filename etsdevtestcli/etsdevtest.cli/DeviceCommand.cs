
using System;
using System.CommandLine;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Itgmbh.AutoDevTest;

namespace etsdevtest.cli;

class DeviceCommand : Command
{
    static Option<int> GetTimeoutOpt(int aDefaultTimeoutInMs)
    {
        return new Option<int>(["-t", "--timeout"], () => aDefaultTimeoutInMs, "Provide a timeout in MS to wait");
    }

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
            var srcSia = new IndividualAddressArgument();
            var dstSia = new Argument<byte>("New line address");
            this.Add(srcSia);
            this.Add(dstSia);
            this.SetHandler((src, dst) =>
            {
                appInstance.Get().ChangeDeviceAddress(src, dst);
            }, srcSia, dstSia);
        }
    }

    class SwuCommand : Command
    {
        public SwuCommand(AppInstance appInstance) : base("swu", "Start software package update")
        {
            var iaArg = new IndividualAddressArgument();
            Add(iaArg);
            var fileArg = new Argument<string>("file", "File path to the .knxfwp file");
            Add(fileArg);
            this.SetHandler((ia, file) =>
            {
                if (!appInstance.Get().StartFirmwareUpdate(ia, file))
                {
                    return Task.FromResult(CommandProcessor.kFailedToStartCommand);
                }

                return Task.FromResult(CommandProcessor.kNoError);
            }, iaArg, fileArg);
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
            var srcSia = new IndividualAddressArgument(aDefault: false);
            this.Add(srcSia);
            var snArg = new SerialnumberArgument(aDefault: true);
            Add(snArg);
            var pwArg = new Argument<string>("pw", () => null, "password to use, if not provided will not configure password");
            Add(pwArg);
            var optTimeout = GetTimeoutOpt(100000);
            Add(optTimeout);
            this.SetHandler((src, sn, pw, timeout) =>
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
                        state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, timeout);
                        Console.WriteLine($"Serialnumber resolve state '{state}'");
                        ProcessState(src, state, "Serialnumber resolving failed");
                    }
                    else
                    {
                        Console.WriteLine($"Download file by programming mode");
                    }

                    appInstance.Get().StartOnlineOperation(src, OnlineOperationType.DownloadFull);

                    state = appInstance.Get().WaitUntilOnlineOperationIsComplete(src, timeout);
                    Console.WriteLine($"Download state '{state}'");
                    ProcessState(src, state, "Download failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    error = CommandProcessor.kDownloadFailed;
                }
                return Task.FromResult(error);
            }, srcSia, snArg, pwArg, optTimeout);
        }
    }

    class ListCommand : Command
    {
        string IAToString(ushort aia)
        {
            return $"{aia >> 12}.{(aia >> 8) & 0xf}.{aia & 0xff}";
        }

        public ListCommand(AppInstance appInstance) : base("list", "Show all available devices")
        {
            this.SetHandler(() =>
            {
                appInstance.Get();
                Console.WriteLine("{0,-8} {1,-3} {2,-13} {3, 0}", "Sia", "Type", "Serialnumber", "(Name)");
                Console.WriteLine(new string('-', 30));
                foreach (var sia in appInstance.Get().GetAllDevices())
                {
                    var infos = appInstance.Get().GetDeviceInformation(sia, [InfoCommand.Infos.MediumType.ToString(), InfoCommand.Infos.Name.ToString(), InfoCommand.Infos.SerialNumber.ToString()]);
                    Console.WriteLine("{0,-8} {1,-3} {2,-13} ({3, 0})", IAToString(sia), infos[0], infos[2], infos[1]);
                }
            });
        }
    }

    class StateCommand : Command
    {
        public StateCommand(AppInstance appInstance) : base("state", "Get Current state of device")
        {
            var iaArg = new IndividualAddressArgument(aDefault: false);
            Add(iaArg);
            var timeoutArg = new Argument<int>("timeout", () => 0, "Provide timeout to wait for completion if running in MS");
            Add(timeoutArg);

            this.SetHandler((ia, timeout) =>
            {
                var state = appInstance.Get().WaitUntilOnlineOperationIsComplete(ia, timeout);
                Console.WriteLine($"Device {ia} is in state {state}");
            }, iaArg, timeoutArg);
        }
    }

    class SerialnumberCommand : Command
    {
        public SerialnumberCommand(AppInstance appInstance) : base("sn", "set serialnumber of sia")
        {
            var srcSia = new IndividualAddressArgument(aDefault: false);
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
            var srcSia = new IndividualAddressArgument(aDefault: false);
            Add(srcSia);
            var snArg = new SerialnumberArgument();
            Add(snArg);
            var pwArg = new Argument<string>("certificate", "Certificate string (DMC Code or KNX:S:<sn>;P:<pw>)");
            Add(pwArg);
            this.SetHandler(appInstance.SetDeviceCertificate, srcSia, snArg, pwArg);
        }
    }

    class InfoCommand : Command
    {
        public enum Infos
        {
            ManufacturerId,
            HardwareId,
            ApplicationProgramId,
            Name,
            Description,
            MediumType,
            Security,
            SerialNumber
        }

        public InfoCommand(AppInstance appInstance) : base("info", "retrieve device information")
        {
            var iaArg = new IndividualAddressArgument();
            Add(iaArg);
            var infoArg = new Argument<Infos>("info", "provide the information to return");
            Add(infoArg);

            this.SetHandler((ia, info) =>
            {
                Console.WriteLine($"Get '{info}' from '{ia}'");
                foreach (var val in appInstance.Get().GetDeviceInformation(ia, [info.ToString()]))
                {
                    Console.WriteLine($"'{val}'");
                }
            }, iaArg, infoArg);
        }
    }

    public DeviceCommand(AppInstance aAppInstance) : base("device", "commands related to manage a device")
    {
        AddCommand(new ParametersCommand(aAppInstance));
        AddCommand(new ChangeAddress(aAppInstance));
        AddCommand(new DownloadCommand(aAppInstance));
        AddCommand(new CertificateCommand(aAppInstance));
        AddCommand(new SerialnumberCommand(aAppInstance));
        AddCommand(new ListCommand(aAppInstance));
        AddCommand(new InfoCommand(aAppInstance));
        AddCommand(new StateCommand(aAppInstance));
        AddCommand(new SwuCommand(aAppInstance));
    }

}