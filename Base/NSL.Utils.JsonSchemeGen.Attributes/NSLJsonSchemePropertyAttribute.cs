using System;

namespace NSL.Utils.JsonSchemeGen.Attributes
{
    public class NSLJsonSchemePropertyAttribute : Attribute
    {
        public string Description { get; set; }

        public string Name { get; set; }
    }
}
