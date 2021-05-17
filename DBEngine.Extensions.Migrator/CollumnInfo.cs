using DBEngine.DBMigrator.ConfigurationAttributes;
using System;
using System.Reflection;

namespace DBEngine.DBMigrator
{
    public class CollumnInfo
    {
        public MemberInfo Property { get; set; }

        public Type Type { get => type; set { type = value; } }

        public SQLTypeData SqlType { get => _sqlType ?? DBCollumnType.GetSQLTypeData(this); private set => _sqlType = value; }

        public DBCollumnAttribute CollumnAttribute;

        public DBAppendCollumnAttribute AppendCollumnAttribute;

        public DBCollumnForeignAttribute ForeignAttribute;

        public DBAppendTableAttribute AppendTableAttribute;

        public DBCollumnForeignKeyAttribute[] ForeignKeyAttributes;

        public Func<object, object> Getter;

        public Action<object, object> Setter;

        public bool IsMigrationType;

        public bool IsAppendTable;

        public bool IsAppendCollumn;

        public MigrationTypeInfo MigrationTypeInfo;

        private Type type;
        private SQLTypeData _sqlType;

        public static CollumnInfo GetSqlCollumnInfo(string collumnName, string sqlType, bool isNullable)
        {
            return new CollumnInfo()
            {
                CollumnAttribute = new DBCollumnAttribute(collumnName),
                SqlType = DBCollumnType.GetSQLTypeData(sqlType, isNullable)
            };
        }


        public override string ToString()
        {
            return $"{CollumnAttribute?.Name} ({Property?.Name})";
        }
    }
}
