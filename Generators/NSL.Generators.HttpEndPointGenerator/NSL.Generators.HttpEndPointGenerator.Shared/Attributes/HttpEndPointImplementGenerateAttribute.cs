using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpEndPointImplementGenerateAttribute : Attribute
    {
        public HttpEndPointImplementGenerateAttribute(Type _interface, bool saveNames = false, bool fullTypePath = true)
        {
            Interface = _interface;
            SaveNames = saveNames;
            FullTypePath = fullTypePath;
        }

        public Type Interface { get; }
        public bool SaveNames { get; }
        public bool FullTypePath { get; }
    }
}
