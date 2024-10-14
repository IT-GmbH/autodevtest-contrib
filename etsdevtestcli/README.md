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

### Configuration

Set configuration to enable easier interaction with current commands:

Show available configurations:

```
$ etsdevtest config list
```

Get the configuration (example ets executable path)

```
$ etsdevtest config get ExecutablePath
```

Set the a configuration (example ets executable path)

```
$ etsdevtest config set ExecutablePath "C:\Users\user\Documents\projects\ets\archive\ETS6 v6.3.7775\ETS6.exe"
```

### Open ETS6

To open and run commands with ETS6, ensure first the AutoDevTest ETS6 App is installed
and works. Also ensure ETS6 is not running.

After you can open a shell to work with ETS6 via tool.

```
$ etsdevtest shell
```

List available projects:

```
> project list
Id         Project
---------------
P-0477     My-Existing-Project
Done
```

Open an existing project:

```
> start My-Existing-Project -p <password>
Done
> open My-Existing-Project
Done
```

Show parameters of a device via sia `1.0.1`

```
> device parameters 1.0.1
Done
```