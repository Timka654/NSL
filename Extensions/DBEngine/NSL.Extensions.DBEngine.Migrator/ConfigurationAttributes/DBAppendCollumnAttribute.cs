using System;

namespace NSL.Extensions.DBEngine.Migrator.ConfigurationAttributes
{
    /// <summary>
    /// Помечает поле для добавления в структуру таблицы дополнительных столбцов, используется вместе с DBCollumnForeignAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DBAppendCollumnAttribute : Attribute
    {
        /// <summary>
        /// Название столбца
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Тип столбца
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Значение по умолчанию для столбца
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Инкрементировать столбец (актуально для значений которые находяться в List и не имеют ключа)
        /// </summary>
        public bool AutoIncrement { get; set; }

        private int IncrementValue { get; set; }

        public int Increment()
        {
            return ++IncrementValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Название столбца</param>
        /// <param name="type">Тип столбца</param>
        public DBAppendCollumnAttribute(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
