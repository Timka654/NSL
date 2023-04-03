using NSL.UDP.Info;
using STUN;
using System.Collections.Generic;

namespace NSL.UDP.Interface
{
    public interface ISTUNOptions
    {
        List<StunServerInfo> StunServers { get; }

        STUNQueryType StunQueryType { get; set; }
    }
}
