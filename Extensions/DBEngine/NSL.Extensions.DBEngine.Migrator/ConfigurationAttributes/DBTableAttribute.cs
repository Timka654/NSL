using System;
using System.Linq;

namespace NSL.Extensions.DBEngine.Migrator.ConfigurationAttributes
{
    /// <summary>
    /// Указывает основные параметры таблицы
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DBTableAttribute : Attribute
    {
        /// <summary>
        /// Название таблицы
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Префикс для вставки в названия
        /// </summary>
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Название столбца идентификатора
        /// </summary>
        public string IdCollumn { get; set; } = "id";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название таблицы</param>
        /// <param name="idCollumn">Название столбца идентификатора</param>
        public DBTableAttribute(string name, params string[] prefixes)
        {
            Name = name;
            Prefixes = prefixes.ToArray();
        }
    }
}
