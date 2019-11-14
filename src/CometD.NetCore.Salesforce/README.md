# CometD.NetCore.Salesforce

[![Build status](https://ci.appveyor.com/api/projects/status/baalfhs6vvc38icc?svg=true)](https://ci.appveyor.com/project/kdcllc/cometd-netcore-salesforce)
[![NuGet](https://img.shields.io/nuget/v/CometD.NetCore.Salesforce.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
[![MyGet](https://img.shields.io/myget/kdcllc/v/CometD.NetCore.Salesforce.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

Add the following to the project

```csharp
    dotnet add package CometD.NetCore.Salesforce
```

This library includes:

- `ResilientStreamingClient` class to create an event bus for Salesforce
- `IResilientForceClient` wrapper class with Resilience for `NetCoreForce.Client`

Complete Sample App using this library can be found at [Bet.BuildingBlocks.SalesforceEventBus](https://github.com/kdcllc/Bet.BuildingBlocks.SalesforceEventBus)
