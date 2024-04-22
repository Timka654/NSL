using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpEndPointImplementGenerateAttribute : Attribute
    {
        public HttpEndPointImplementGenerateAttribute(Type _interface) { }
    }
}
