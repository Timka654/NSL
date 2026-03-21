using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HttpEndPointExcludeUsingAttribute : Attribute
    {

        public HttpEndPointExcludeUsingAttribute(params string[] name)
        {
            Name = name;
        }

        public string[] Name { get; }
    }
}
