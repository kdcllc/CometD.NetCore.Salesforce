# `salesforce` CLI authentication tool

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/cometd-netcore-salesforce/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/baalfhs6vvc38icc?svg=true)](https://ci.appveyor.com/project/kdcllc/cometd-netcore-salesforce)
[![NuGet](https://img.shields.io/nuget/v/salesforce.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/kdcllc/shield/salesforce/latest)](https://f.feedz.io/kdcllc/kdcllc/packages/salesforce/latest/download)

_Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/kcllc/nuget/index.json)._

## Summary

This is a dotnet cli Saleforce Refresh and Access Tokens Generation tool.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install DotNetCore Cli `salesforce` tool

```bash
    dotnet tool install salesforce -g
```

To verify the installation run:

```bash
     dotnet tool list -g
```

## Usage of Salesforce dotnet cli tool

There are several ways to run this cli tool.

This tool will open web browser and will require you to log in with your credentials to Salesforce portal in order to retrieve the tokens.

1. From any location with Consumer Key and Secret provided

```bash
    # specify the custom login url
    salesforce get-tokens --key:{key} --secret:{secret} --login:https://login.salesforce.com --verbose:information

    # use default login url
    salesforce get-tokens --key:{key} --secret:{secret} --verbose
```

2. Running the tool in the directory that contains `appsettings.json` file with configurations

```bash
    salesforce get-tokens --section:Salesforce
```

Note: required configurations are as follows:

```json
  "Salesforce": {
    "ClientId": "",
    "ClientSecret": "",
    "LoginUrl": ""
  }
````

3. Running with Azure Vault

a.) Location with `appsettings.json` file

```json
    "AzureVault": {
     "BaseUrl": "https://{name}.vault.azure.net/"
    },
```

```bash
    salesforce get-tokens --verbose:debug
```
b.) From any location

Or specify url within the dotnet cli tool like so:

```cmd
    salesforce get-tokens --azure https://{name}.vault.azure.net/"
```

## Tools possible switches

- `--key` or `-k` (Salesforce `Consumer Key`)
- `--secret` or `-s` (Salesforce `Consumer Secret`)
- `--login` or `-l` (Salesforce login url)
- `--azure` or `-a` (Azure Vault Url)
- `--azureprefix` or `ax` ([Use Environment prefix for Azure vault](https://github.com/kdcllc/Bet.AspNetCore/blob/d8ff3b7bfb13817bc2b6b768d91ea19a2bc865a5/src/Bet.Extensions.AzureVault/AzureVaultKeyBuilder.cs#L24))
- `--configfile` or `-c` (Specify configuration file)
- `--verbose:debug` or `--verbose:information` or `--verbose:trave`
- `--usesecrets` or `us` (Usually a Guid Id of the project that contains the secret)
- `--environment` or `-e` (Production, Development, Stage)
- `--section` or `-sn` (The root for the tools configuration the default is `Salesforce`)

## Build self contained

[rid-catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

```bash
    # windows
    dotnet build -r win-x64 -c Release  -p:PackAsTool=false
    dotnet publish -r win-x64 -c Release  -p:PackAsTool=false -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -f netcoreapp3.0 -o ../../packages

    # linux
    dotnet build -r linux-x64 -c Release  -p:PackAsTool=false
    dotnet publish -r linux-x64 -c Release  -p:PackAsTool=false -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=false -f netcoreapp3.0 -o ../../packages
```
