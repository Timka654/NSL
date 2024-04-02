using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SelectGenerateModelJoinAttribute : Attribute
    {
        public SelectGenerateModelJoinAttribute(string model, params string[] includeModels)
        {
                
        }
    }
}
