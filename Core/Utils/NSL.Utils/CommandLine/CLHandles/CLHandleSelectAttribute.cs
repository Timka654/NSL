using System;

namespace NSL.Utils.CommandLine.CLHandles
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CLHandleSelectAttribute : Attribute
    {
        public CLHandleSelectAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
