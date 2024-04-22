using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class HttpEndPointContainerGenerateAttribute : Attribute
    {
        public HttpEndPointContainerGenerateAttribute() { }

        public HttpEndPointContainerGenerateAttribute(string path) { }
    }
}
