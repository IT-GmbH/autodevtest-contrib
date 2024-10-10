# ETS6AutoDevTest CLI

Provide a cli and a shell to use the AutoDevTest ETS6 App.

This is a C# app and only supports ETS6 for now.

* Requires [dotnet](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) tool.
* Uses `net4.8`
* Only supported on windows
* Copy the *Itgmbh.AutoDevTest.** files into *./lib*
    * *lib\Itgmbh.AutoDevTest.Contract.dll*
    * *lib\Itgmbh.AutoDevTest.Framework.dll*
* For development it is recommended to also copy the xml files:
    * *lib\Itgmbh.AutoDevTest.Contract.xml*
    * *lib\Itgmbh.AutoDevTest.Framework.xml*

## Run

`dotnet run -p .\etsdevtool.cli\ -- --help`

## Run Tests

`dotnet tests`

## Create MSI for `etsdevtest`

Create msi:

`dotnet build -c Release`

msi is located in *./installer/DevTestToolCli.msi*.
After installation, it should be possible to run:

```
etsdevtest -h
```

> Be aware, currently installing multiple versions will lead to
> multiple entries in the program list.

## Commands

tbd.