using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinarySerializer
{
    /// <summary>
    /// Аттрибут с данными предкомпиляции структуры
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BinaryPreCompileAttribute : Attribute
    {
        /// <summary>
        /// Начальный размер буффера при серриализации
        /// </summary>
        public int InitialSize { get; private set; } = 32;

        /// <summary>
        /// Схема предкомпиляции
        /// </summary>
        public string Scheme { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme">Схема предкомпиляции</param>
        /// <param name="initialSize">Начальный размер буффера при серриализации</param>
        public BinaryPreCompileAttribute(string scheme, int initialSize = 32)
        {
            InitialSize = initialSize;
            Scheme = scheme;

        }
    }
}
