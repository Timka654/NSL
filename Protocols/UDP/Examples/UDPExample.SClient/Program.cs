using Open.Nat;
using UDPExample;

var discoverer = new NatDiscoverer();

var device = await discoverer.DiscoverDeviceAsync( PortMapper.Pmp, new CancellationTokenSource());

//var exMappings = device.GetAllMappingsAsync();
try
{
    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 5553, 5553));
}
catch (MappingException mex)
{

}

new ReceiverExample();

while (true)
{
    Thread.Sleep(100);
}