using System.Diagnostics;
using System.Runtime.CompilerServices;
using WixSharp;
using WixSharp.Bootstrapper;
using WixSharp.CommonTasks;
using WixToolset.Dtf.WindowsInstaller;
using File = WixSharp.File;

[assembly: InternalsVisibleTo(assemblyName: "installer.aot")] // assembly name + '.aot suffix


internal class Program
{

    static async Task Main(string[] args)
    {
        var entity = new WixEntity();
        entity.Id = "Environment";
        entity.Attributes.Add("hello", "world");

        var env = new EnvironmentVariable(name: "PATH", value: "[INSTALLDIR]");
        env.System = false;

        var binaryPath = @"..\etsdevtest.cli\bin\Release\net48";

        var mainExe = new File($"{binaryPath}\\etsdevtest.cli.exe");
        mainExe.TargetFileName = "etsdevtest.exe";

        List<WixEntity> Files = new List<WixEntity>{
            mainExe
        };
        foreach (var file in Directory.GetFiles(binaryPath))
        {
            var path = file.PathGetFullPath();
            if (path.EndsWith(".dll"))
            {
                Files.Add(new File(path));
            }
        }

        foreach (var file in Files)
        {
            Console.WriteLine("Added File {0}", file.Name);
        }
        var installDir = new Dir(@"%ProgramFiles%\itgmbh\contrib\devtesttoolcli", Files.ToArray());

        var project =
            new ManagedProject("DevTestToolCli",
                env,
                installDir);

        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
        project.MajorUpgradeStrategy.RemoveExistingProductAfter = Step.InstallInitialize;

        project.Version = new Version(0, 0, 1);
        project.LicenceFile = "license.rtf";
        project.GUID = new Guid("6fe30b47-1077-43ad-3000-1221ba258555");

        project.BuildMsi();
    }
}