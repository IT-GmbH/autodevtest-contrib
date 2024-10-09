using Hanssens.Net;
using System;
using System.ComponentModel;

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

    LocalStorage mlocalStorage = new LocalStorage();
    IEts6Factory mEts6Factory;

    public Config(IEts6Factory aEts6Factory, LocalStorage? aLocalStorage = null)
    {
        if (aLocalStorage != null)
        {
            mlocalStorage = aLocalStorage;
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
        mEts6Factory.ExecutablePath = Get(IConfig.Types.ExecutablePath);
        mEts6Factory.ProjectStore = Get(IConfig.Types.ProjectStore);
    }

    void ProcessSet(IConfig.Types aKey, string aValue)
    {
        switch (aKey)
        {
            case IConfig.Types.ProjectStore:
                mEts6Factory.ProjectStore = aValue;
                break;
            case IConfig.Types.ExecutablePath:
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
        mlocalStorage.Store(GetString(aKey), aValue);
        ProcessSet(aKey, aValue);
        mlocalStorage.Persist();
    }

    public void Clear()
    {
        mlocalStorage.Clear();
        mlocalStorage.Persist();
    }
};