using Microsoft.CodeAnalysis;

namespace NSL.Generators.RpcGenerator.Core
{
    internal class RpcResultData
    {
        public ITypeSymbol Type { get; set; }

        /// <summary>
        /// Optional BinaryGenerator model name (null = full serialization).
        /// </summary>
        public string BinaryModel { get; set; }
    }
}
