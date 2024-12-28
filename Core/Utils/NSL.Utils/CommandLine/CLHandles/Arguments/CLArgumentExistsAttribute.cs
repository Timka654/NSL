using System;

namespace NSL.Utils.CommandLine.CLHandles.Arguments
{
    /// <summary>
    /// Set true if argument exists to property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CLArgumentExistsAttribute : Attribute
    {
        public CLArgumentExistsAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
