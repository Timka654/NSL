using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class RPCCustomMemberIgnoreAttribute : Attribute
    {
        public RPCCustomMemberIgnoreAttribute(params string[] members)
        {
            Members = members;
        }

        public string[] Members { get; }
    }
}
