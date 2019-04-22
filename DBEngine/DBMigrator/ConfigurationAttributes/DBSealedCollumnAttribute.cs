using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEngine.DBMigrator.ConfigurationAttributes
{
    /// <summary>
    /// Помечает поле указывая члены закрытого класса (тот которые не возможно изменить для добавления аттрибутов) которые необходимо мигроровать в таблицу, используется вместе с DBCollumnForeignAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DBSealedCollumnAttribute : Attribute
    {
        /// <summary>
        /// Название поля в таблице
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Тип поля
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Название поля с учетом регистра
        /// </summary>
        public string MemberName { get; private set; }

        public DBSealedCollumnAttribute(string name, Type type, string memberName)
        {
            Name = name;
            Type = type;
            MemberName = memberName;
        }
    }
}
