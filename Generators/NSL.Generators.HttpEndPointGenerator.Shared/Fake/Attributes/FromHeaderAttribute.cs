using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes
{
    /// <summary>
    /// Fake attribute class for generator success build
    /// </summary>
    public class FromHeaderAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
