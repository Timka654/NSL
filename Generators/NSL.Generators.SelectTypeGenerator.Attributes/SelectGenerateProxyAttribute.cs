using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class SelectGenerateProxyAttribute : Attribute
    {
        /// <summary>
        /// From unconfigured models to child object model change on select
        /// </summary>
        /// <param name="toModel"></param>
        public SelectGenerateProxyAttribute(string toModel) { }

        /// <summary>
        /// Configure model to child object model change on select
        /// </summary>
        /// <param name="fromModel"></param>
        /// <param name="toModel"></param>
        public SelectGenerateProxyAttribute(string fromModel, string toModel) { }
    }
}
