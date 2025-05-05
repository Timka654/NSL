using System;

namespace NSL.Database.EntityFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EntityExcludeModelAttribute : Attribute
    {
    }
}
