

using System;
using System.CommandLine;
using System.Diagnostics;
using System.Reflection;

namespace etsdevtest.cli;

public class VersionCommand : Command
{
    static Version mEtsAppVersion;
    static Version mEtsDevTestCliVersion;

    public static Version EtsAppVersion()
    {
        if (mEtsAppVersion == null)
        {
            mEtsAppVersion = Assembly.GetAssembly(typeof(Itgmbh.AutoDevTest.Ets6)).GetName().Version;
        }
        return mEtsAppVersion;
    }

    public static Version GetVersion()
    {
        if (mEtsDevTestCliVersion == null)
        {
            mEtsDevTestCliVersion = Assembly.GetAssembly(typeof(VersionCommand)).GetName().Version;
        }
        return mEtsDevTestCliVersion;
    }

    public static Version FullVersion()
    {
        return new Version(
            GetVersion().Major,
            GetVersion().Minor,
            GetVersion().Build,
            EtsAppVersion().Build
        );
    }

    public VersionCommand() : base("version", "retrieve current app version and ets6 app dependency")
    {
        this.SetHandler(() =>
        {
            Console.WriteLine($"Version: {FullVersion()}");
            Console.WriteLine($"> ETS App: {EtsAppVersion()}");
        });
    }
}