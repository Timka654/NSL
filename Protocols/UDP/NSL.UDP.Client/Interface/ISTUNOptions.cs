using NSL.UDP.Client.Info;
using STUN;
using System.Collections.Generic;

namespace NSL.UDP.Client.Interface
{
    public interface ISTUNOptions
    {
        List<StunServerInfo> StunServers { get; }

        STUNQueryType StunQueryType { get; set; }
    }
}
