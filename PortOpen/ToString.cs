using Lost.PortForwarding;

namespace PortOpen;

static class ToString
{
	public static string Str(PortMapper mapper) => mapper switch
	{
		PortMapper.Pmp => "PMP",
		PortMapper.Upnp => "UPnP",
		_ => "Unknown",
	};

	public static string Str(Protocol protocol) => protocol switch
	{
		Protocol.Tcp => "TCP",
		Protocol.Udp => "UDP",
		_ => "Unknown",
	};
}
