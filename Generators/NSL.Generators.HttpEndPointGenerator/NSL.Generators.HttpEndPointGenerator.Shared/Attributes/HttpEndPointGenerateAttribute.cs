using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class HttpEndPointGenerateAttribute : Attribute
    {
        public HttpEndPointGenerateAttribute(Type returnType)
        {
            ReturnType = returnType;
        }

        public HttpEndPointGenerateAttribute(Type returnType, string urlTemplate)
        {
            ReturnType = returnType;
            UrlTemplate = urlTemplate;
        }

        public Type ReturnType { get; }
        public string UrlTemplate { get; }
    }
}
