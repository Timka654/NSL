using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    /// <summary>
    /// Add generate packet argument for send
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class NSLPHGenArgAttribute : Attribute
    {
        public string Name { get; set; }

        public NSLPHGenArgAttribute(Type itemType)
        {

        }

        public NSLPHGenArgAttribute(Type itemType, string binaryModel)
        {

        }
    }
}
