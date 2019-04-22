using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEngine.DBMigrator
{
    public class DBCollumnType
    {
        private static Dictionary<Type, SQLTypeData> Types = new Dictionary<Type, SQLTypeData>()
        {
            { typeof(byte), new SQLTypeData("[tinyint]", "NOT NULL",(v)=>{ return v.ToString(); }, ()=>{ return 0; }) },
            { typeof(byte?), new SQLTypeData("[tinyint]", "NULL",(v)=>{ return v?.ToString() ?? "NULL"; }, ()=>{ return 0; })  },

            { typeof(short), new SQLTypeData("[smallint]", "NOT NULL",(v)=>{ return v.ToString(); }, ()=>{ return 0; })  },
            { typeof(short?), new SQLTypeData("[smallint]", "NULL",(v)=>{ return v?.ToString() ?? "NULL"; }, ()=>{ return 0; })  },

            { typeof(int), new SQLTypeData("[int]", "NOT NULL",(v)=>{ return v.ToString(); }, ()=>{ return 0; })  },
            { typeof(int?), new SQLTypeData("[int]", "NULL",(v)=>{ return v?.ToString() ?? "NULL"; }, ()=>{ return 0; })  },

            { typeof(long), new SQLTypeData("[bigint]", "NOT NULL",(v)=>{ return v.ToString(); }, ()=>{ return 0; })  },
            { typeof(long?), new SQLTypeData("[bigint]", "NULL",(v)=>{ return v?.ToString() ?? "NULL"; }, ()=>{ return 0; })  },

            { typeof(bool), new SQLTypeData("[bit]", "NOT NULL",(v)=>{ return ((bool)v? 1 : 0).ToString(); }, ()=>{ return 0; })  },
            { typeof(bool?), new SQLTypeData("[bit]", "NULL",(v)=>{ return v == null ? "NULL" : ((bool)v? 1 : 0).ToString(); }, ()=>{ return 0; })  },

            { typeof(float), new SQLTypeData("[float]", "NOT NULL",(v)=>{ return v.ToString().Replace(',','.'); }, ()=>{ return 0; }) },
            { typeof(float?), new SQLTypeData("[float]", "NULL",(v)=>{ return v?.ToString().Replace(',','.') ?? "NULL"; }, ()=>{ return 0; }) },

            { typeof(double), new SQLTypeData("[decimal](12,8)", "NOT NULL",(v)=>{ return v.ToString().Replace(',','.'); }, ()=>{ return 0; }) },
            { typeof(double?), new SQLTypeData("[decimal](12,8)", "NULL",(v)=>{ return v?.ToString().Replace(',','.') ?? "NULL"; }, ()=>{ return 0; }) },

            { typeof(string), new SQLTypeData("[nvarchar](max)", "NULL",(v)=>{ return v == null ? "NULL" : $"'{((string)v)}'"; }, ()=>{ return 0; }) },

            { typeof(Enum), new SQLTypeData("[int]", "NOT NULL",(v)=>{ return ((int)v).ToString(); }, ()=>{ return 0; }) },
            { typeof(DateTime), new SQLTypeData("[smalldatetime]", "NOT NULL",(v)=>{ return ((DateTime)v).ToString(); }, ()=>{ return DateTime.MinValue; }) },
            { typeof(TimeSpan), new SQLTypeData("[bigint]", "NOT NULL",(v)=>{ return ((TimeSpan)v).TotalMilliseconds.ToString(); }, ()=>{ return default(TimeSpan); }) },
        };

        private static Dictionary<KeyValuePair<string, bool>, Type> SqlTypes = new Dictionary<KeyValuePair<string, bool>, Type>()
        {
            { new KeyValuePair<string,bool>("tinyint",false),typeof(byte)},
            { new KeyValuePair<string,bool>("tinyint",true),typeof(byte?)},

            { new KeyValuePair<string,bool>("smallint",false),typeof(short)},
            { new KeyValuePair<string,bool>("smallint",true),typeof(short?)},

            { new KeyValuePair<string,bool>("int",false),typeof(int)},
            { new KeyValuePair<string,bool>("int",true),typeof(int?)},

            { new KeyValuePair<string,bool>("bigint",false),typeof(long)},
            { new KeyValuePair<string,bool>("bigint",true),typeof(long?)},

            { new KeyValuePair<string,bool>("bit",false),typeof(bool)},
            { new KeyValuePair<string,bool>("bit",true),typeof(bool?)},

            { new KeyValuePair<string,bool>("float",false),typeof(float)},
            { new KeyValuePair<string,bool>("float",true),typeof(float?)},

            { new KeyValuePair<string,bool>("decimal",false),typeof(double)},
            { new KeyValuePair<string,bool>("decimal",true),typeof(double?)},

            { new KeyValuePair<string,bool>("nvarchar",false),typeof(string)},
            { new KeyValuePair<string,bool>("nvarchar",true),typeof(string)},
        };

        public static SQLTypeData GetSQLTypeData(CollumnInfo collumn)
        {
            var t = collumn.CollumnAttribute.Type ?? collumn.Type;
            if (!Types.ContainsKey(t))
                throw new InvalidCastException($"Collumn:{collumn.Property.Name} have not supported SqlType");
            return Types[t];
        }

        public static SQLTypeData GetSQLTypeData(Type type)
        {
            if (!Types.ContainsKey(type))
                throw new InvalidCastException($"Type:{type} have not supported SqlType");
            return Types[type];
        }

        internal static SQLTypeData GetSQLTypeData(string sqlType, bool isNullable)
        {
            return Types[SqlTypes[new KeyValuePair<string, bool>(sqlType, isNullable)]];
        }
    }
}