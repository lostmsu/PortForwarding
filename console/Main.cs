using System.Text;

using Lost.PortForwarding;


var t = Task.Run(async () =>
{
    var nat = new NatDiscoverer();
    var cts = new CancellationTokenSource(5000);
    var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts.Token);

    var sb = new StringBuilder();
    var ip = await device.GetExternalIPAsync();

	foreach (var mapping in await device.GetAllMappingsAsync())
	{
		Console.WriteLine(mapping);
	}

    sb.AppendFormat("\nYour IP: {0}", ip);
    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700, "Lost.PortForwarding (temporary)"));
    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1601, 1701, "Lost.PortForwarding (Session lifetime)"));
    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1602, 1702, MappingLifetime.Permanent, "Lost.PortForwarding (Permanent lifetime)"));
    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 1603, 1703, new(TimeSpan.FromSeconds(20)), "Lost.PortForwarding (Manual lifetime)"));
    sb.AppendFormat("\nAdded mapping: {0}:1700 -> 127.0.0.1:1600\n", ip);
    sb.AppendFormat(
        "\n+------+-------------------------------+--------------------------------+------------------------------------+-------------------------+");
    sb.AppendFormat("\n| PROT | PUBLIC (Reacheable)		   | PRIVATE (Your computer)		| Descriptopn						|						 |");
    sb.AppendFormat(
        "\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
    sb.AppendFormat("\n|	  | IP Address		   | Port   | IP Address			| Port   |									| Expires				 |");
    sb.AppendFormat(
        "\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");
    foreach (var mapping in await device.GetAllMappingsAsync())
    {
        sb.AppendFormat("\n|  {5} | {0,-20} | {1,6} | {2,-21} | {3,6} | {4,-35}|{6,25}|",
            ip, mapping.PublicPort, mapping.PrivateIP, mapping.PrivatePort, mapping.Description,
            mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP", mapping.Expiration.ToLocalTime());
    }
    sb.AppendFormat(
        "\n+------+----------------------+--------+-----------------------+--------+------------------------------------+-------------------------+");

    sb.AppendFormat("\n[Removing TCP mapping] {0}:1700 -> 127.0.0.1:1600", ip);
    await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, 1600, 1700));
    sb.AppendFormat("\n[Done]");

    Console.WriteLine(sb.ToString());
    /*
                    var mappings = await device.GetAllMappingsAsync();
                    var deleted = mappings.All(x => x.Description != "Lost.PortForwarding Testing");
                    Console.WriteLine(deleted
                        ? "[SUCCESS]: Test mapping effectively removed ;)"
                        : "[FAILURE]: Test mapping wan not removed!");
    */
});

try
{
    t.Wait();
}
catch (AggregateException e)
{
    if (e.InnerException is NatDeviceNotFoundException)
    {
        Console.WriteLine("Not found");
        Console.WriteLine("Press any key to exit...");
    }
}
Console.ReadKey();
