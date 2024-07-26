using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryTypeIOGenerator.Attributes
{
    [Obsolete("use NSLBIO attributes for use actual logic")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIODataIgnoreAttribute : Attribute
    {
        public BinaryIODataIgnoreAttribute(params string[] @for)
        {

        }
    }
}
