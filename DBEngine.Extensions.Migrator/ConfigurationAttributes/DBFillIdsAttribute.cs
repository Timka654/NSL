using System;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DBFillIdsAttribute : Attribute
    {
    }
}
