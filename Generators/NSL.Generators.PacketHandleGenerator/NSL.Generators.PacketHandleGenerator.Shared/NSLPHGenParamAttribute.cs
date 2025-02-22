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
        public Type ItemType { get; }
        public string BinaryModel { get; }

        public NSLPHGenArgAttribute(Type itemType)
        {
            ItemType = itemType;
        }

        public NSLPHGenArgAttribute(Type itemType, string binaryModel)
        {
            ItemType = itemType;
            BinaryModel = binaryModel;
        }
    }
}
