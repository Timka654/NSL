using System;

namespace NSL.Extensions.RPC.Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RPCMemberIgnoreAttribute : Attribute
    {
        public RPCMemberIgnoreAttribute()
        {

        }
    }
}
