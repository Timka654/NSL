using System;

namespace NSL.Utils.CommandLine.CLHandles.Arguments
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CLArgumentAttribute : Attribute
    {
        public CLArgumentAttribute(string name, Type type, bool optional = false)
        {
            Name = name;
            Type = type;
            Optional = optional;
        }

        public string Name { get; }

        public Type Type { get; }

        public bool Optional { get; }

        public string Description { get; set; }
    }
}
