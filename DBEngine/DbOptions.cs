using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace DBEngine
{
    public class XmlDbOptions
    {
        [XmlElement(ElementName = "root")]
        public DbOptions root;
    }

    /// <summary>
    /// Тип базы данных
    /// </summary>
    public enum DBType
    {
        None,
        MySql,
        MsSql
    }

    /// <summary>
    /// Настройки базы данных
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// IP адресс к базе данных
        /// </summary>
        [XmlElement(ElementName = "host")]
        public string Host { get;set; }
        
        /// <summary>
        /// Логин к базе данных
        /// </summary>
        [XmlElement(ElementName = "user")]
        public string User { get; set; }

        /// <summary>
        /// Пароль к базе данных
        /// </summary>
        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// Название базы данных
        /// </summary>
        [XmlElement(ElementName = "name")]
        public string DbName { get; set; }

        /// <summary>
        /// Тип базы данных MySQl, MsSql
        /// </summary>
        [XmlElement(ElementName = "type")]
        public DBType DbType { get; set; }

        /// <summary>
        /// Размер пула подключений (используется для инициализации определенного кол-ва подключений для паралельных запросов)
        /// </summary>
        [XmlElement(ElementName = "pool_size")]
        public int PoolSize { get; set; }

        /// <summary>
        /// В некоторых базах данных помимо основных есть другие параметры которых необходимо задать
        /// </summary>
        [XmlElement(ElementName = "other_params")]
        public string OtherParams { get;set; }
    }
}
