
using System;
using System.CommandLine;

namespace etsdevtest.cli;

class ProjectCommand : Command
{
    class ListCommand : Command
    {
        public ListCommand(IEts6Factory aEts6Factory, IConfig aConfig) : base("list", "show all projects in project store")
        {
            this.SetHandler(() =>
            {
                Console.WriteLine("List Project form project store '{0}'", aConfig.Get(IConfig.Types.ProjectStore, ""));

                Console.WriteLine("{0,-10} {1,5}", "Id", "Project");
                Console.WriteLine(new string('-', 15));
                foreach (var project in aEts6Factory.Projects())
                {
                    Console.WriteLine("{0,-10} {1,5}", project.Key, project.Value);
                }
            });
        }
    }


    class OpenCommand : Command
    {
        public OpenCommand(AppInstance aAppInstance, IConfig aConfig) : base("open", "Open the provided project")
        {
            var projectName = StartCommand.GetProjectArg(aConfig);
            var optPassword = new Option<string>(["-p", "--password"], () => null, "project password");
            var optForce = new Option<bool>(["-f", "--force"], () => false, "force open new project");
            this.Add(projectName);
            this.Add(optPassword);
            this.Add(optForce);
            this.SetHandler(aAppInstance.Open, projectName, optPassword, optForce);
        }
    }

    class CloseCommmand : Command
    {
        public CloseCommmand(AppInstance aAppInstance) : base("close", "Close currently running ETS")
        {
            this.SetHandler(aAppInstance.Close);
        }
    }

    public ProjectCommand(IEts6Factory aEts6Factory, IConfig aConfig, AppInstance aAppInstance) : base("project", "Manage ets6 projects")
    {
        Add(new ListCommand(aEts6Factory, aConfig));
        Add(new StartCommand(aAppInstance, aConfig));
        Add(new OpenCommand(aAppInstance, aConfig));
        Add(new CloseCommmand(aAppInstance));
    }
}