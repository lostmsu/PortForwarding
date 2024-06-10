![Logo](https://github.com/lontivero/Open.NAT/raw/gh-pages/images/logos/128.jpg)

[![NuGet](https://img.shields.io/nuget/v/Lost.PortForwarding.svg)](https://www.nuget.org/packages/Lost.PortForwarding/)

Lost.PortForwarding
======

Lost.PortForwarding is a lightweight and easy-to-use class library to allow port forwarding in NAT devices that support UPNP (Universal Plug & Play) and/or PMP (Port Mapping Protocol).


Goals
-----
NATed computers cannot be reached from outside and this is particularly painful for peer-to-peer or friend-to-friend software.
The main goal is to simplify communication amoung computers behind NAT devices that support UPNP and/or PMP providing a clean
and easy interface to get the external IP address and map ports and helping you to achieve peer-to-peer communication.

+ Tested with .NET  _YES_
+ Tested with Mono  _YES_

How to use?
-----------
With nuget :
> **Install-Package Lost.PortForwarding**

Go on the [nuget website](https://www.nuget.org/packages/Lost.PortForwarding/) for more information.

Example
--------

The simplest scenario:

```c#
var discoverer = new NatDiscoverer();
var device = await discoverer.DiscoverDeviceAsync();
var ip = await device.GetExternalIPAsync();
Console.WriteLine("The external IP Address is: {0} ", ip);
```

The following piece of code shows a common scenario: It starts the discovery process for a NAT-UPNP device and onces discovered it creates a port mapping. If no device is found before ten seconds, it fails with NatDeviceNotFoundException.


```c#
var discoverer = new NatDiscoverer();
var cts = new CancellationTokenSource(10000);
var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700, "The mapping name"));
```

For more info please check the [Wiki](https://github.com/lontivero/Lost.PortForwarding/wiki)

Documentation
-------------
+ Why Lost.PortForwarding? Here you have [ten reasons](https://github.com/lontivero/Lost.PortForwarding/wiki/Why-Lost.PortForwarding) that make Lost.PortForwarding a good candidate for you projects
+ [Visit the Wiki page](https://github.com/lontivero/Lost.PortForwarding/wiki)

Development
-----------
Lost.PortForwarding is been developed by [Lucas Ontivero](http://geeks.ms/blogs/lontivero) ([@lontivero](http://twitter.com/lontivero)).
You are welcome to contribute code. You can send code both as a patch or a GitHub pull request.
