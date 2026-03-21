using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpEndPointIncludeUsingAttribute : Attribute
    {
        public HttpEndPointIncludeUsingAttribute(params string[] name)
        {
            Name = name;
        }

        public string[] Name { get; }
    }
}
