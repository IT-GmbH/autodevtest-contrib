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
    }

    public string GetString(Types aKey);

    public string Get(Types aKey, string aDefault = null);

    public void Set(Types aKey, string aValue);

    public void Clear();
}

public class Config : IConfig
{

    LocalStorage mlocalStorage = null;
    IEts6Factory mEts6Factory;

    public static LocalStorage GetDefaultLocalStorage()
    {
        var config = new LocalStorageConfiguration();
        var storageFile = Environment.GetEnvironmentVariable("ETSDEVTESTCLI_STORAGE");
        if (storageFile != null)
        {
            config.Filename = storageFile;
        }
        Console.WriteLine("PATH: {0}", config.Filename);
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
                if (!File.Exists(aValue))
                {
                    throw new ArgumentException($"'{aValue}' is not a file");
                }
                mEts6Factory.ProjectStore = aValue;
                break;
            case IConfig.Types.ExecutablePath:
                if (!Directory.Exists(aValue))
                {
                    throw new ArgumentException($"'{aValue}' is not an directory");
                }
                mEts6Factory.ExecutablePath = aValue;
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