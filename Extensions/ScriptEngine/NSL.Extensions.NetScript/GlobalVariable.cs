using System;

namespace NSL.Extensions.NetScript
{
    public struct GlobalVariable
    {
        public string name;
        public string type;
        public object value;

        /// <summary>
        /// Интерфейс описывающий глобальную переменную скрипта
        /// </summary>
        /// <param name="variable_name">Имя переменной</param>
        /// <param name="full_type_name">Полное имя типа</param>
        public GlobalVariable(string variable_name, string full_type_name)
        {
            name = variable_name;
            type = full_type_name;
            value = null;
        }

        /// <summary>
        /// Интерфейс описывающий глобальную переменную скрипта
        /// </summary>
        /// <param name="variable_name">Имя переменной</param>
        /// <param name="full_type_name">Полное имя типа</param>
        /// <param name="value">Начальное значение переменной</param>
        public GlobalVariable(string variable_name, string full_type_name, object value) : this(variable_name, full_type_name)
        {
            this.value = value;
        }

        /// <summary>
        /// Интерфейс описывающий глобальную переменную скрипта
        /// </summary>
        /// <param name="variable_name">Имя переменной</param>
        /// <param name="type">Тип переменной</param>
        public GlobalVariable(string variable_name, Type type) : this(variable_name, type.FullName.Replace("+", "."))
        { }

        /// <summary>
        /// Интерфейс описывающий глобальную переменную скрипта
        /// </summary>
        /// <param name="variable_name">Имя переменной</param>
        /// <param name="type">Тип переменной</param>
        /// <param name="value">Начальное значение переменной</param>
        public GlobalVariable(string variable_name, Type type, object value) : this(variable_name, type.FullName.Replace("+","."),value)
        { }
        }
}
