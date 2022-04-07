using System;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    /// <summary>
    /// Пометить поле ссылочного типа что-бы интегрировать в текущую таблицу все столбцы
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DBAppendTableAttribute : Attribute
    {
    }
}
