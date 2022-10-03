using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Attributes
{
    public class RPCTypeHandleAttribute : Attribute
    {
        public RPCTypeHandleAttribute(Type forType)
        {
            ForType = forType;
        }

        public Type ForType { get; }
    }
}
