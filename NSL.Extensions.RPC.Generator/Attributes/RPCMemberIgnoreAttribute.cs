using System;
using System.Collections.Generic;
using System.Text;

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
