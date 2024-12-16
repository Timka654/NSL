using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    /// <summary>
    /// Set Result type for request
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NSLPHGenResultAttribute : Attribute
    {
        public NSLPHGenResultAttribute(Type type)
        {

        }

        public NSLPHGenResultAttribute(Type type, string binaryModel)
        {

        }
    }
}
