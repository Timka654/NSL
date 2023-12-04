using System;

namespace NSL.Generators.HttpEndPointGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpEndPointImplementGenerateAttribute : Attribute
    {
        public HttpEndPointImplementGenerateAttribute(Type _interface) { }
    }
}
