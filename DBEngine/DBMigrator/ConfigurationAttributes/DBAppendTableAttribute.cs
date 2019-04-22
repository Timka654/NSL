using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
