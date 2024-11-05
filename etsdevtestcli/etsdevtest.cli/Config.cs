using Hanssens.Net;
using System;
using System.ComponentModel;
using System.IO;

namespace etsdevtest.cli;

public interface IConfig
{
    public enum Types
    {
        [Description("projectstore")]
        ProjectStore,
        [Description("executable")]
        ExecutablePath,
        [Description("defaultproject")]
        DefaultProject,
        [Description("defaultpassword")]
        DefaultProjectPassword,
        [Description("etsdir")]
        ETSDirectory
    }

    public string GetString(Types aKey);

    public string GetConfigFile();

    public string Get(Types aKey, string aDefault = null);

    public void Set(Types aKey, string aValue);

    public void Clear();
}

public class Config : IConfig
{

    LocalStorage mlocalStorage = null;
    IEts6Factory mEts6Factory;
    string mConfigStoreFile;

    public LocalStorage GetDefaultLocalStorage()
    {
        var config = new LocalStorageConfiguration();
        var storageFile = Environment.GetEnvironmentVariable("ETSDEVTESTCLI_STORAGE");
        if (storageFile != null)
        {
            config.Filename = storageFile;
        }
        mConfigStoreFile = config.Filename;
        return new LocalStorage(config);
    }

    public Config(IEts6Factory aEts6Factory, LocalStorage? aLocalStorage = null)
    {
        if (aLocalStorage != null)
        {
            mlocalStorage = aLocalStorage;
        }
        else
        {
            mlocalStorage = GetDefaultLocalStorage();
        }
        mlocalStorage.Load();
        mEts6Factory = aEts6Factory;
        LoadConfig();
    }

    public string GetConfigFile()
    {
        return mConfigStoreFile;
    }

    public string GetString(IConfig.Types aValue)
    {
        var field = aValue.GetType().GetField(aValue.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute == null ? aValue.ToString() : attribute.Description;
    }

    string GetDefault(IConfig.Types aKey)
    {
        switch (aKey)
        {
            case IConfig.Types.ProjectStore:
                return mEts6Factory.ProjectStore;
            case IConfig.Types.ExecutablePath:
                return mEts6Factory.ExecutablePath;
        }
        return "";
    }

    public string Get(IConfig.Types aKey, string aDefault = null)
    {
        try
        {
            return mlocalStorage.Get<string>(GetString(aKey));
        }
        catch (ArgumentNullException)
        {
            if (aDefault == null)
            {
                aDefault = GetDefault(aKey);
            }
            if (aDefault == null)
            {
                return "";
            }
            return aDefault;
        }
    }

    void LoadConfig()
    {
        try
        {
            mEts6Factory.ExecutablePath = Get(IConfig.Types.ExecutablePath);
            mEts6Factory.ProjectStore = Get(IConfig.Types.ProjectStore);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.Error.WriteLine("Failed to load configuration, {0}", ex);
        }
    }

    void ProcessSet(IConfig.Types aKey, string aValue)
    {
        switch (aKey)
        {
            case IConfig.Types.ProjectStore:
                if (!Directory.Exists(aValue))
                {
                    throw new ArgumentException($"'{aValue}' is not a directory");
                }
                mEts6Factory.ProjectStore = aValue;
                break;
            case IConfig.Types.ExecutablePath:
                if (!File.Exists(aValue))
                {
                    throw new ArgumentException($"'{aValue}' is not an file");
                }
                mEts6Factory.ExecutablePath = aValue;
                break;
            case IConfig.Types.ETSDirectory:
                if (!Directory.Exists(aValue))
                {
                    throw new ArgumentException($"'{aValue}' is not a directory");
                }
                Set(IConfig.Types.ExecutablePath, $"{aValue}\\ETS6.exe");
                var projectStore = $"{aValue}\\ProjectStore";
                if(!Directory.Exists(projectStore)) {
                    Directory.CreateDirectory(projectStore);
                }
                Set(IConfig.Types.ProjectStore, projectStore);
                break;
        }
    }

    public void Set(IConfig.Types aKey, string aValue)
    {
        if (aValue == null)
        {
            return;
        }
        ProcessSet(aKey, aValue);
        mlocalStorage.Store(GetString(aKey), aValue);
        mlocalStorage.Persist();
    }

    public void Clear()
    {
        mlocalStorage.Clear();
        mlocalStorage.Persist();
    }
};