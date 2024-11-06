using System;
using System.Security.Cryptography;
using Itgmbh.AutoDevTest;

namespace etsdevtest.cli;

/// <summary>
/// Lazy load the AutoDevTestApp instance by only loading
/// when required.
/// </summary>
public class AppInstance : IDisposable
{
    IAutoDevTestApp mApp;
    IEts6Factory mEts6Factory;
    IEts mEts6;
    IConfig mConfig;
    string mActiveProject;
    string mActiveProjectPassword;

    public AppInstance(IEts6Factory aEts6Factory, IConfig aConfig)
    {
        mEts6Factory = aEts6Factory;
        mConfig = aConfig;
    }

    public bool IsEtsStarted()
    {
        return mEts6 != null;
    }

    public IEts GetEts()
    {
        if (mEts6 == null)
        {
            Start(mConfig.Get(IConfig.Types.DefaultProject), mConfig.Get(IConfig.Types.DefaultProjectPassword, ""));
        }
        return mEts6;
    }

    public IAutoDevTestApp Get()
    {
        if (mApp == null)
        {
            Open(mConfig.Get(IConfig.Types.DefaultProject));
        }
        return mApp;
    }

    public void SetDeviceCertificate(ushort aSia, Serialnumber aSerialnumber, string aPassword)
    {
        Get().SetDeviceSerialNumber(aSia, aSerialnumber.Value);
        Get().AddRawDeviceCertificate($"KNX:S:{aSerialnumber.String};P:{aPassword}".ToUpper());
    }

    public void Reopen()
    {
        Close();
        Open(mActiveProject, mActiveProjectPassword);
    }

    public void Start(string projectName, string password = null, string importFromFile = null, int timeoutMilliseconds = 30000)
    {
        if (mEts6 != null)
        {
            throw new Exception("Ets6 already started");
        }
        if (password == string.Empty || password == null)
        {
            password = mConfig.Get(IConfig.Types.DefaultProjectPassword, null);
        }
        Console.WriteLine($"Start Ets6 project: '{projectName}' password '{password}' import '{importFromFile}' timeout {timeoutMilliseconds}ms");
        mEts6 = mEts6Factory.Start(projectName, password, importFromFile, timeoutMilliseconds);
    }

    public void Exit()
    {
        Dispose();
    }

    /// <summary>
    /// Open a project by name
    /// </summary>
    /// <param name="projectName">The project to open</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void Open(string projectName, string password = null, bool aForce = false)
    {
        if (mApp != null && !aForce)
        {
            throw new Exception($"Already active project open '{mActiveProject}'");
        }
        mApp = null;

        if (!IsEtsStarted())
        {
            Start(projectName, password);
        }

        Console.WriteLine("Open project '{0}'", projectName);
        mApp = GetEts().GetOpenProject(projectName);
        mActiveProject = projectName;
        mActiveProjectPassword = password;
    }

    /// <summary>
    /// Close the project (TMP: currently not possible)
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (mEts6 != null)
        {
            mEts6.Dispose();
        }
        mEts6 = null;
        mApp = null;
    }
}