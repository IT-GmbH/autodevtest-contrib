using System;
using System.Security.Cryptography;
using Itgmbh.AutoDevTest;

namespace etsdevtest.cli;

/// <summary>
/// Lazy load the AutoDevTestApp instance by only loading
/// when required.
/// </summary>
public class AppInstance : IDisposable {
    IAutoDevTestApp mApp;
    IEts6Factory mEts6Factory;
    IEts mEts6;
    IConfig mConfig;
    string mActiveProject;

    public AppInstance(IEts6Factory aEts6Factory, IConfig aConfig) {
        mEts6Factory = aEts6Factory;
        mConfig = aConfig;
    }

    public IEts GetEts() {
        if(mEts6 == null) {
            Start(mConfig.Get(IConfig.Types.DefaultProject), mConfig.Get(IConfig.Types.DefaultProjectPassword, ""));
        }
        return mEts6;
    }

    public IAutoDevTestApp Get() {
        if(mApp == null) {
            Open(mConfig.Get(IConfig.Types.DefaultProject));
        }
        return mApp;
    }

    public void Start(string projectName, string password = null, string importFromFile = null, int timeoutMilliseconds = 30000) {
        if(mEts6 != null) {
            throw new Exception("Ets6 already started");
        }
        if(password == string.Empty) {
            password = null;
        }
        mEts6 = mEts6Factory.Start(projectName, password, importFromFile, timeoutMilliseconds);
    }

    /// <summary>
    /// Open a project by name
    /// </summary>
    /// <param name="projectName">The project to open</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void Open(string projectName) {
        Console.WriteLine("Open project '{0}'", projectName);
        if(mApp != null) {
            throw new Exception($"Already active project open '{mActiveProject}'");
        }
        mApp = GetEts().GetOpenProject(projectName);
        mActiveProject = projectName;
    }

    /// <summary>
    /// Close the project (TMP: currently not possible)
    /// </summary>
    public void Close() {
        mApp = null;
        mActiveProject = null;
    }

    public void Dispose()
    {
        if(mEts6 != null) {
            mEts6.Dispose();
        }
        Close();
    }
}