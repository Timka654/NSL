using System;
using System.Collections.Generic;
using System.Text;

namespace BinarySerializer
{
    /// <summary>
    /// Аттрибут указывающий схему для члена класса
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BinarySchemeAttribute : Attribute
    {
        /// <summary>
        /// Название схемы
        /// </summary>
        public string SchemeName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemeName">Название схемы</param>
        public BinarySchemeAttribute(string schemeName)
        {
            SchemeName = schemeName;
        }
    }
}
