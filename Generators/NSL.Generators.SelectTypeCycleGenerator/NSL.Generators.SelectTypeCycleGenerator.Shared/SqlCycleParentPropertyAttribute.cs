using System;

namespace NSL.Entity.SelectCycleGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SqlCycleHierarchyAttribute : Attribute
    {
        public string ParentIdProperty { get; }
        public string IdProperty { get; }

        public SqlCycleHierarchyAttribute(string parentIdProperty = "ParentId", string idProperty = "Id")
        {
            ParentIdProperty = parentIdProperty;
            IdProperty = idProperty;
        }
    }
}
