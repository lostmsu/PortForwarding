using Lost.PortForwarding;

using System.Diagnostics;
using System.Globalization;

using static PortOpen.ToString;
using static System.FormattableString;

if (args.Length == 0)
{
	Console.Error.WriteLine("Usage: PortOpen [duration] [tcp] [udp] <port1> [<port2> ...]");
	Console.Error.WriteLine("  duration: port lease duration (e.g., 30s, 5m, 1h). Default is 1h.");
	Console.Error.WriteLine("  tcp: open TCP ports.");
	Console.Error.WriteLine("  udp: open UDP ports.");
	Console.Error.WriteLine("  port: port numbers between 1 and 65535.");
	return 1;
}

var ports = new List<int>();
if (ParseArgs(args, ports,
				out TimeSpan duration,
				out bool tcp, out bool udp) is int parseResult
				&& parseResult != 0)
{
	return parseResult;
}

var nat = new NatDiscoverer();
var cts = new CancellationTokenSource(5000);
var pmp = nat.DiscoverDeviceAsync(PortMapper.Pmp, cts.Token);
var upnp = nat.DiscoverDeviceAsync(PortMapper.Upnp, cts.Token);
var devices = new List<Task<NatDevice>> { pmp, upnp };

var protocols = new List<Protocol>();
if (tcp) protocols.Add(Protocol.Tcp);
if (udp) protocols.Add(Protocol.Udp);

var results = await Task.WhenAll(ports.SelectMany(
	port => protocols.Select(protocol => OpenPort(port, protocol))));

return results.Max();

async Task<int> OpenPort(int port, Protocol protocol)
{
	var tasks = devices.Select(dev => OpenPortOnDevice(port, protocol, dev)).ToList();
	var best = MapResult.UNKRNOWN_ERROR;
	while (tasks.Count > 0)
	{
		var completed = await Task.WhenAny(tasks);
		tasks.Remove(completed);
		var result = await completed;
		if (result == MapResult.OK)
			return (int)MapResult.OK;
	}
	var results = await Task.WhenAll(tasks);
	var resultStr = string.Join("\t", results);
	Console.Error.WriteLine(Invariant($"{Str(protocol)}\t{port}\t{results}"));
	return (int)best;
}

async Task<MapResult> OpenPortOnDevice(int port, Protocol protocol, Task<NatDevice> deviceTask)
{
	try
	{
		var device = await deviceTask;
		await device.CreatePortMapAsync(new(protocol,
			privatePort: port, publicPort: port,
			duration, "PortOpen Tool"));
		Console.WriteLine(Invariant($"{Str(protocol)}\t{port}\t{Str(device.PortMapper)}\tOK"));
		return MapResult.OK;
	}
	catch (NatDeviceNotFoundException)
	{
		return MapResult.DEVICE_NOT_FOUND;
	}
	catch (Exception ex)
	{
		Trace.WriteLine(ex);
		return MapResult.UNKRNOWN_ERROR;
	}
}

static TimeSpan? TryParseDuration(string input)
{
	if (string.IsNullOrEmpty(input))
		return null;

	if (!int.TryParse(input.Substring(0, input.Length - 1),
					NumberStyles.Integer, CultureInfo.InvariantCulture,
					out int duration) || duration <= 0)
		return null;

	return input.Last() switch
	{
		's' => TimeSpan.FromSeconds(duration),
		'm' => TimeSpan.FromMinutes(duration),
		'h' => TimeSpan.FromHours(duration),
		_ => null,
	};
}

static int ParseArgs(string[] args, List<int> ports,
					out TimeSpan duration,
					out bool tcp, out bool udp)
{
	duration = TimeSpan.FromHours(1);
	tcp = udp = false;
	foreach (string arg in args)
	{
		if (arg.Equals("tcp", StringComparison.OrdinalIgnoreCase))
		{
			tcp = true;
		}
		else if (arg.Equals("udp", StringComparison.OrdinalIgnoreCase))
		{
			udp = true;
		}
		else if (int.TryParse(arg, out int port) && port > 0 && port <= 65535)
		{
			ports.Add(port);
		}
		else if (TryParseDuration(arg) is TimeSpan durationValue)
		{
			duration = durationValue;
		}
		else
		{
			Console.Error.WriteLine($"Invalid argument: {arg}");
			return 1;
		}
	}

	if (!tcp && !udp)
		tcp = udp = true;

	if (ports.Count == 0)
	{
		Console.Error.WriteLine("No ports specified.");
		return 1;
	}

	return 0;
}

enum MapResult
{
	OK,
	DEVICE_NOT_FOUND = 10,
	UNKRNOWN_ERROR = 100,
}