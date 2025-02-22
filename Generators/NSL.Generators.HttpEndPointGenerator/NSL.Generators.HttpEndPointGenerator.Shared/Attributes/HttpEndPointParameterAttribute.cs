using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HttpEndPointParameterAttribute : Attribute
    {
        public HttpEndPointParameterAttribute(GenHttpParameterEnum type = GenHttpParameterEnum.Normal)
        {
            Type = type;
        }

        public GenHttpParameterEnum Type { get; }
    }

    public enum GenHttpParameterEnum
    {
        /// <summary>
        /// Json body content(this use as default)
        /// </summary>
        Normal,

        /// <summary>
        /// Processing members on 0 depth in model as independ parameter
        /// </summary>
        Particle
    }
}
