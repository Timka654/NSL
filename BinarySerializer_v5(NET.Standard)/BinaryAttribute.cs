using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer
{
    /// <summary>
    /// Аттрибут с данными об учаснике серриализации
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class BinaryAttribute : Attribute
    {
        /// <summary>
        /// Размер типа (если не указан TypeSizeName)
        /// </summary>
        public int TypeSize { get; set; } = 0;

        /// <summary>
        /// Название учасника содержащего размер типа
        /// </summary>
        public string TypeSizeName { get; set; } = null;

        /// <summary>
        /// Размер массива (если не указан ArraySizeName)
        /// </summary>
        public int ArraySize { get; set; } = 0;

        /// <summary>
        /// Название учасника содержащего размер массива
        /// </summary>
        public string ArraySizeName { get; set; } = null;

        /// <summary>
        /// Тип обработчика учасника
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Тип обработчика учасника</param>
        public BinaryAttribute(Type type)
        {
            Type = type;
        }
    }
}
