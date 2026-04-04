using Microsoft.CodeAnalysis;
using NSL.Generators.RpcGenerator.Shared;
using System.Collections.Generic;

namespace NSL.Generators.RpcGenerator.Core
{
    internal class RpcContainerData
    {
        public ITypeSymbol InterfaceType { get; set; }

        public ITypeSymbol NetworkDataType { get; set; }

        public NSLRPCDirection Direction { get; set; }

        public List<RpcMethodData> Methods { get; set; } = new List<RpcMethodData>();
    }
}
