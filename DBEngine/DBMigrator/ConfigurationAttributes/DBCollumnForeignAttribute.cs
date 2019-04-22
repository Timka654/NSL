using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    /// <summary>
    /// Помечает поле для создания связанной таблице по шаблону типа поля
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DBCollumnForeignAttribute : Attribute
    {
        /// <summary>
        /// Названия связующего ключа которому будет присвоено значение идентификатора текущей таблицы
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Название созданой таблицы
        /// </summary>
        public string Table { get; internal set; }

        /// <summary>
        /// Указывает что тип поля нельзя модифицировать для установки аттрибутов миграции
        /// Необходимо добавить аттрибуты типа DBSealedCollumnAttribute и указать значения полей
        /// </summary>
        public bool Sealed { get; set; }


        public string[] ForeignPrefixes { get; internal set; }
        ///// <summary>
        ///// Названия столбцов в текущей таблице с помощью которых будет поддерживаться связь с дочерней таблицей (SourceKeys[x] == DestinationKeys[x])
        ///// </summary>
        //public string[] SourceKeys { get; set; }

        ///// <summary>
        ///// Название столбцов в текущей таблице с помощью которых будет поддерживаться связь с родительской таблицей (SourceKeys[x] == DestinationKeys[x])
        ///// </summary>
        //public string[] DestinationKeys { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Названия связующего ключа которому будет присвоено значение идентификатора текущей таблицы</param>
        /// <param name="table">Название созданой таблицы</param>
        public DBCollumnForeignAttribute(string name, string table, params string [] foreignPrefixes)
        {
            Name = name;

            Table = table;

            ForeignPrefixes = foreignPrefixes.ToArray();
        }
    }
}
