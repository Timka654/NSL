using System;

namespace NSL.Extensions.DBEngine.Migrator.ConfigurationAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DBFillIdsAttribute : Attribute
    {
    }
}
