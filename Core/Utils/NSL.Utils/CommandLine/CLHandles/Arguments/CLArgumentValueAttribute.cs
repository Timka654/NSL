using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Utils.CommandLine.CLHandles.Arguments
{
    /// <summary>
    /// Set value to property from input arguments
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CLArgumentValueAttribute : Attribute
    {
        public CLArgumentValueAttribute(string name, object defaultValue = default)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public object DefaultValue { get; }
    }
}
