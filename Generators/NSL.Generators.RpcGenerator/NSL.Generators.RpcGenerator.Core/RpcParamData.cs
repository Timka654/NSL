using Microsoft.CodeAnalysis;

namespace NSL.Generators.RpcGenerator.Core
{
    internal class RpcParamData
    {
        public ITypeSymbol Type { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Optional BinaryGenerator model name (null = full serialization).
        /// </summary>
        public string BinaryModel { get; set; }
    }
}
