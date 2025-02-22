using System;

namespace NSL.Generators.FillTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FillTypeGenerateProxyAttribute : Attribute
    {
        /// <summary>
        /// From unconfigured models to child object model change on fill
        /// </summary>
        /// <param name="toModel"></param>
        public FillTypeGenerateProxyAttribute(string toModel)
        {
            ToModel = toModel;
        }

        /// <summary>
        /// Configure model to child object model change on fill
        /// </summary>
        /// <param name="fromModel"></param>
        /// <param name="toModel"></param>
        public FillTypeGenerateProxyAttribute(string fromModel, string toModel)
        {
            FromModel = fromModel;
            ToModel = toModel;
        }

        public string ToModel { get; }
        public string FromModel { get; }
    }
}
