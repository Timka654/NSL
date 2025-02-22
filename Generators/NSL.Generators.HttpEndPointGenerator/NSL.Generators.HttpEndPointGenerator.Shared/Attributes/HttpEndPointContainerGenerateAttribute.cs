using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class HttpEndPointContainerGenerateAttribute : Attribute
    {
        public HttpEndPointContainerGenerateAttribute() { }

        public HttpEndPointContainerGenerateAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
