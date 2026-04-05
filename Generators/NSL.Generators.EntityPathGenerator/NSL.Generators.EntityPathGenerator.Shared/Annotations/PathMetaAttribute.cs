using System;

namespace NSL.Generators.EntityPathGenerator.Shared.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PathMetaAttribute : Attribute
    {
        public string Key { get; }
        public object Value { get; }

        public PathMetaAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}