using System.Net;

namespace NSL.UDP.Client.Interface
{
    public interface IBindingUDPOptions : ISTUNOptions
    {
        string BindingIP { get; set; }
        int BindingPort { get; set; }

        int ReceiveChannelCount { get; set; }

        IPAddress GetBindingIPAddress();
        IPEndPoint GetBindingIPEndPoint();
    }
}
