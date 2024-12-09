using System;

namespace NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes
{
    /// <summary>
    /// Fake attribute class for generator success build
    /// </summary>
    public class FromQueryAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
