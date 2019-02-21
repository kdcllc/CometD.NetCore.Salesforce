# CometD .NET Core implementation of Salesforce Platform events
[![Build status](https://ci.appveyor.com/api/projects/status/baalfhs6vvc38icc?svg=true)](https://ci.appveyor.com/project/kdcllc/cometd-netcore-salesforce)

This repo contains the CometD .NET Core implementation for Salesforce Platform events. 
These events can be subscribed to and listened to by your custom `Event Listener`.

1. `CometD.NetCore2.Salesforce`
     - Salesforce Platform Events as Event Bus [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers).
     - [Reusable Building Blocks and sample application that listens to Salesforce push events](https://github.com/kdcllc/Bet.BuildingBlocks.SalesforceEventBus).
2. DotNet Cli tool `AuthApp`
   - This dotnet cli tool allows for retrieval of `Access Token` and `Refresh Token` to be used by any other application. Please refer to [How Are Apps Authenticated with the Web Server OAuth Authentication Flow](https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_understanding_web_server_oauth_flow.htm)


## Installation

- To include this library inside of your project run nuget package installation
> Install-Package CometD.NetCore2.Salesforce
Or
> dotnet add package CometD.NetCore.Salesforce
- To Inatall Salesforce Cli tool globally

> dotnet tool install salesforce -g

Then to see if it was installed correctly:

> dotnet tool list -g

## Saleforce Setup
[Video](https://www.youtube.com/watch?v=L6OWyCfQD6U)
1. Sing up for development sandbox with Saleforce: [https://developer.salesforce.com/signup](https://developer.salesforce.com/signup).
2. Create Connected App in Salesforce.
3. Create a Platform Event.

### Create Connected App in Salesforce
1. Setup -> Quick Find -> manage -> App Manager -> New Connected App.
2. Basic Info:

![info](./img/new-app-basic-info.jpg)

3. API (Enable OAuth Settings):
![settings](./img/new-app-api-auth.jpg)

4. Retrieve `Consumer Key` and `Consumer Secret` to be used within the Test App

### Create a Platform Event
1. Setup -> Quick Find -> Events -> Platform Events -> New Platform Event:

![event](./img/new-platform-event.jpg)

2. Add Custom Field

![event](./img/new-platform-event-field.jpg)

(note: use sandbox custom domain for the login to workbench in order to install this app within your production)

Use workbench to test the Event [workbench](https://workbench.developerforce.com/login.php?startUrl=%2Finsert.php)
## AuthApp
[Use login instead of test](https://github.com/developerforce/Force.com-Toolkit-for-NET/wiki/Web-Server-OAuth-Flow-Sample#am-i-using-the-test-environment)
Simple application that provides with Web Server OAuth Authentication Flow to retrieve 
`Access Token` and `Refresh Token` to be used within the application.

## Special thanks to the following projects and contributors:
- [Oyatel/CometD.NET](https://github.com/Oyatel/CometD.NET)
- [nthachus/CometD.NET](https://github.com/nthachus/CometD.NET)
- [tdawgy/CometD.NetCore](https://github.com/tdawgy/CometD.NetCore)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [Chris Woolum](https://github.com/cwoolum)
