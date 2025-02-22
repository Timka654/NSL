using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NSLPHGenImplAttribute : Attribute
    {
        /// <summary>
        /// Generate static method for get client/requestProcessor for <see cref="NSLHPDirTypeEnum.Send"/> direction
        /// </summary>
        public bool IsStaticNetwork { get; set; }

        /// <summary>
        /// Generate delegates response scheme for <see cref="NSLHPDirTypeEnum.Send"/ > direction methods
        /// </summary>
        public bool DelegateOutputResponse { get; set; }

        /// <summary>
        /// Enum for implement packets
        /// </summary>
        public Type PacketsEnum { get; set; }

        /// <summary>
        /// INetworkClient client data class
        /// </summary>
        public Type NetworkDataType { get; set; }

        /// <summary>
        /// Direction for generate methods/handles
        /// </summary>
        public NSLHPDirTypeEnum Direction { get; set; }

        /// <summary>
        /// Modifier for generated methods handle/send
        /// </summary>
        public NSLAccessModifierEnum Modifier { get; set; }

        public bool IsAsync { get; set; }
        public string[] Models { get; }

        public NSLPHGenImplAttribute(params string[] models)
        {
            Models = models;
        }
    }
}
