using System;

namespace NSL.Generators.SelectTypeGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SelectGenerateModelJoinAttribute : Attribute
    {
        public SelectGenerateModelJoinAttribute(string model, params string[] includeModels)
        {
            Model = model;
            IncludeModels = includeModels;
        }

        public string Model { get; }
        public string[] IncludeModels { get; }
    }
}
