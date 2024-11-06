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
        var storageFilePath = @"%AppData%\itgmbh\contrib\devtesttoolcli";
        var binaryPath = @"..\etsdevtest.cli\bin\Release\net48";

        var entity = new WixEntity();
        entity.Id = "Environment";
        entity.Attributes.Add("hello", "world");

        var env = new EnvironmentVariable(name: "PATH", value: "[INSTALLDIR]");
        env.System = false;
        env.Part = EnvVarPart.last;

        var storage = new EnvironmentVariable(name: "ETSDEVTESTCLI_STORAGE", value: $"{storageFilePath}\\.localstorage");
        storage.System = false;

        var mainExe = new File($"{binaryPath}\\etsdevtest.cli.exe");
        mainExe.TargetFileName = "etsdevtest.exe";

        List<WixEntity> Entities = new List<WixEntity>{
            mainExe
        };
        foreach (var file in Directory.GetFiles(binaryPath))
        {
            var path = file.PathGetFullPath();
            if (path.EndsWith(".dll"))
            {
                Entities.Add(new File(path));
                Console.WriteLine("Added File {0}", path);
            }
        }

        var installDir = new Dir(@"%ProgramFiles%\itgmbh\contrib\devtesttoolcli", Entities.ToArray());
        var dataDir = new Dir(storageFilePath);

        var project =
            new ManagedProject("DevTestToolCli",
                new LaunchCondition("This application requires .NET Framework 4.8.1 or later.", "Installed OR WIX_IS_NETFRAMEWORK_481_OR_LATER_INSTALLED"),
                env,
                storage,
                installDir,
                dataDir);

        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
        project.MajorUpgradeStrategy.RemoveExistingProductAfter = Step.InstallInitialize;

        project.Version = etsdevtest.cli.VersionCommand.FullVersion();
        project.LicenceFile = "license.rtf";
        project.GUID = new Guid("6fe30b47-1077-43ad-3000-1221ba258555");
        project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;

        Console.WriteLine($"Create version v{project.Version}");

        Compiler.BuildMsi(project, $"setup.v{project.Version}.msi");
    }
}