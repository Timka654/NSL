using System;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    /// <summary>
    /// Пометить структуру для автоматической миграции (в стурктуре обязательно должен быть идентификатор)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DBAutoMigrationAttribute : Attribute
    {
        public int Order { get; set; }
    }
}
