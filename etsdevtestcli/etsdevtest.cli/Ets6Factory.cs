using System.Collections;
using System.Collections.Generic;
using Itgmbh.AutoDevTest;

namespace etsdevtest.cli;

public interface IEts6Factory
{

    IEts Start(string projectName, string password = null, string importFromFile = null, int timeoutMilliseconds = 30000);

    IEnumerable<KeyValuePair<string, string>> Projects();

    string ProjectStore
    {
        get;
        set;
    }

    string ExecutablePath
    {
        get;
        set;
    }

};

public class Ets6Factory : IEts6Factory
{
    public string ProjectStore
    {
        get => Ets6.ProjectStore;
        set => Ets6.ProjectStore = value;
    }

    public string ExecutablePath
    {
        get => Ets6.ExecutablePath;
        set => Ets6.ExecutablePath = value;
    }

    public IEts Start(string projectName, string password = null, string importFromFile = null, int timeoutMilliseconds = 30000)
    {
        return Ets6.Start(projectName, password, importFromFile, timeoutMilliseconds);
    }

    public IEnumerable<KeyValuePair<string, string>> Projects()
    {
        return Ets6.Projects;
    }
}