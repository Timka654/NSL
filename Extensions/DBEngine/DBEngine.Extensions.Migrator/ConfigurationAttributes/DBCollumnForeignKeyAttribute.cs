using System;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DBCollumnForeignKeyAttribute : Attribute
    {
        public string SourceCollumnName { get; internal set; }

        public object SourceValue { get; internal set; }

        public string DestinationCollumnName { get; internal set; }

        public DBCollumnForeignKeyAttribute(string sourceCollumnName, string destinationCollumnName)
        {
            SourceCollumnName = sourceCollumnName;
            DestinationCollumnName = destinationCollumnName;
        }
    }
}
