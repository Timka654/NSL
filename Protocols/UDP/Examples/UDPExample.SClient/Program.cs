using Open.Nat;
using UDPExample;

var discoverer = new NatDiscoverer();

var discoverToken = new CancellationTokenSource();

discoverToken.CancelAfter(6_000);

try
{
	var device = await discoverer.DiscoverDeviceAsync(PortMapper.Pmp, discoverToken);

	//var exMappings = device.GetAllMappingsAsync();
	try
	{
		await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 5553, 5553));
	}
	catch (MappingException mex)
	{

	}

}
catch (TaskCanceledException)
{
}

new ReceiverExample();

while (true)
{
	Thread.Sleep(100);
}