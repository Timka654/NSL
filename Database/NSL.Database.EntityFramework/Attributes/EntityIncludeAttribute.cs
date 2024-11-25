using System;

namespace NSL.Database.EntityFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EntityIncludeAttribute : Attribute
    {
    }
}
