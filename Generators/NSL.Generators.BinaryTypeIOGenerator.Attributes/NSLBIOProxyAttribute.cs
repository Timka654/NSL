using System;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class NSLBIOProxyAttribute : Attribute
    {
        public NSLBIOProxyAttribute(string toModel, params string[] models) { }

        /// <summary>
        /// all not configured models has proxy to this model
        /// </summary>
        /// <param name="toModel"></param>
        public NSLBIOProxyAttribute(string toModel) { }
    }
}
