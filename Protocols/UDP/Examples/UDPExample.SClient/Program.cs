using NSL.Logger;
using Open.Nat;
using UDPExample;


ConsoleLogger logger = new ConsoleLogger();

if (false)
{
	var discoverer = new NatDiscoverer();

	var discoverToken = new CancellationTokenSource();

	discoverToken.CancelAfter(6_000);

	try
	{
		var device = await discoverer.DiscoverDeviceAsync(PortMapper.Pmp | PortMapper.Upnp, discoverToken);

		//var exMappings = device.GetAllMappingsAsync();
		try
		{
			await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 5553, 5553));
		}
		catch (MappingException mex)
        {
            logger.AppendError($"NAT: {mex}");
        }

	}
	catch (NatDeviceNotFoundException natnfoundException)
	{
		logger.AppendError($"NAT: {natnfoundException}");

    }
	catch (TaskCanceledException)
    {
        logger.AppendError($"NAT: Time expired");
    }
}

new ReceiverExample(logger);

while (true)
{
	Thread.Sleep(100);
}