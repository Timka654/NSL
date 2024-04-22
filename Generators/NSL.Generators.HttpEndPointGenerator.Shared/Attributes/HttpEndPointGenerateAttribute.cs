using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpEndPointGenerateAttribute : Attribute
    {
        public HttpEndPointGenerateAttribute(Type returnType)
        {

        }

        public HttpEndPointGenerateAttribute(Type returnType, string urlTemplate)
        {

        }
    }
}
