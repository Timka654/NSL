using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    /// <summary>
    /// Помечает поле для создания столбца в таблице
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DBCollumnAttribute : Attribute
    {
        /// <summary>
        /// Название столбца
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Тип столбца
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название столбца</param>
        public DBCollumnAttribute(string name)
        {
            Name = name;
        }
    }
}
