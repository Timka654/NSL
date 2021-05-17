using System;
using System.Linq;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DBMigrationIgnoreFieldsAttribute : Attribute
    {
        public string[] NameArray { get; private set; }

        public DBMigrationIgnoreFieldsAttribute(params string[] names)
        {
            NameArray = names.ToArray();
        }
    }
}
