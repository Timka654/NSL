using NSL.UDP.Client.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client.Interface
{
    public interface IBindingUDPOptions : ISTUNOptions
    {
        string BindingIP { get; set; }
        int BindingPort { get; set; }

        IPAddress GetBindingIPAddress();
        IPEndPoint GetBindingIPEndPoint();
    }
}
