using System;

namespace NSL.Generators.PacketHandleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class NSLPHGenParamAttribute : Attribute
    {
        public string Name { get; set; }

        public NSLPHGenParamAttribute(Type itemType)
        {

        }

        public NSLPHGenParamAttribute(Type itemType, string binaryModel)
        {

        }
    }
}
