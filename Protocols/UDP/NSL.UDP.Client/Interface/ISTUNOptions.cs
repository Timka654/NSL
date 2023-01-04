using NSL.UDP.Client.Info;
using STUN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client.Interface
{
    public interface ISTUNOptions
    {
        List<StunServerInfo> StunServers { get; }

        STUNQueryType StunQueryType { get; set; }
    }
}
