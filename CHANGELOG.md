# Changelog

### 2021-04-06 v3.0.3

* [Add support for handling 401 Authentication Errors from Salesforce](https://github.com/kdcllc/CometD.NetCore.Salesforce/issues/23) thanks @[apaulro](https://github.com/apaulro)
### 2021-04-06 v3.0.2

* Updated dependencies
    - NetCoreForce.Client
    - netstandard2.1 and net5.0 framework support client support via Microsoft.Bcl.AsyncInterfaces

* [Allow configuration of SF authentication token type](https://github.com/kdcllc/CometD.NetCore.Salesforce/pull/24)
* [Allow injecting a strategy for handling invalid replay Ids](https://github.com/kdcllc/CometD.NetCore.Salesforce/pull/25)
* [Silently fails when replay id is too old.](https://github.com/kdcllc/CometD.NetCore.Salesforce/issues/20)