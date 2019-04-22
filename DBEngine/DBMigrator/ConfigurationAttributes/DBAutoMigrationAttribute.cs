using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
