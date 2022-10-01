using System;

namespace NSL.Extensions.RPC.Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class RPCMemberIgnoreAttribute : Attribute
    {
        public RPCMemberIgnoreAttribute()
        {

        }
    }
}
