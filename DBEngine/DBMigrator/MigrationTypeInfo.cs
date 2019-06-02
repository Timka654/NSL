using DBEngine.DBMigrator.ConfigurationAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBEngine.DBMigrator
{
    public class MigrationTypeInfo
    {
        public Type Type { get; set; }

        public List<CollumnInfo> Collumns { get; private set; }

        public List<CollumnInfo> ExistDbCollumns { get; set; } = new List<CollumnInfo>();

        public List<CollumnInfo> DbCollumns { get; set; } = new List<CollumnInfo>();

        public bool HaveId { get; private set; }

        public bool HaveForeign { get; private set; }

        public string TableName { get; set; }

        public bool IsBaseConfig { get; private set; }

        public string IdKey { get; private set; } = "id";

        public string ForeignKey { get; private set; }

        public object ForeiegnValue { get; private set; }

        public CollumnInfo IdCollumn { get; private set; }

        public CollumnInfo ForeignCollumn { get; private set; }

        public DBCollumnForeignKeyAttribute[] SourceKeys { get; private set; }

        private Dictionary<KeyValuePair<string, object>, object> AppendedValues = new Dictionary<KeyValuePair<string, object>, object>();

        public IEnumerable Values { get; set; }

        public static MigrationTypeInfo LoadType(Type t, IEnumerable values, DBCollumnForeignAttribute foreignKey = null, DBSealedCollumnAttribute[] sealed_collumns = null, string[] prefixes = null, DBCollumnForeignKeyAttribute[] foreignKeys = null)
        {
            var r = new MigrationTypeInfo();

            r.Type = t;

            var tableAttr = r.Type.GetCustomAttribute<DBTableAttribute>();

            if (tableAttr != null)
            {
                r.IdKey = tableAttr.IdCollumn;
                r.TableName = tableAttr.Name;
                if (prefixes == null)
                    prefixes = tableAttr.Prefixes;
            }

            r.Values = values;
            r.Collumns = GetCollumns(t, foreignKey, sealed_collumns, prefixes, foreignKeys);
            r.SourceKeys = foreignKeys;
            r.HaveForeign = !string.IsNullOrEmpty(foreignKey?.Name);
            r.HaveId = r.Collumns.Exists(x => x.CollumnAttribute?.Name == r.IdKey || x.AppendCollumnAttribute?.Name == r.IdKey);

            r.IsBaseConfig = (typeof(DBIdentityEntity)).IsAssignableFrom(t);

            var ignored = (DBMigrationIgnoreFieldsAttribute)Attribute.GetCustomAttribute(t, typeof(DBMigrationIgnoreFieldsAttribute));

            if (tableAttr != null && foreignKey == null && ignored != null)
            {

                r.Collumns = r.Collumns.Where(x => !ignored.NameArray.Contains(x.Property.Name)).ToList();
            }

            if (r.HaveId)
            {
                r.IdCollumn = r.Collumns.FirstOrDefault(x => x.CollumnAttribute?.Name == r.IdKey || x.AppendCollumnAttribute?.Name == r.IdKey);

            }

            if (r.HaveForeign)
            {
                r.ForeignKey = foreignKey.Name;
                r.ForeignCollumn = r.Collumns.FirstOrDefault(x => x.CollumnAttribute?.Name == foreignKey.Name || x.AppendCollumnAttribute?.Name == foreignKey.Name);
            }

            DBCollumnAttribute dbCol = null;
            if (!r.Collumns.All(x =>
            {
                if (x.CollumnAttribute == null)
                    return true;
                dbCol = x.CollumnAttribute;
                return r.Collumns.Count(y => y.CollumnAttribute != null && y.CollumnAttribute.Name == x.CollumnAttribute.Name) == 1;
            }))
            {
                throw new Exception($"{dbCol.Name} have duplicate");
            }

            return r;
        }

        public static void FillIds(MigrationTypeInfo type)
        {
            var list = type.Values as IEnumerable<DBIdentityEntity>;

            list = list.OrderBy(x => x.GetIndex() != 0).OrderBy(x => x.GetIndex()).ToList();

            int temp = 0;

            foreach (var item in list)
            {
                ++temp;
                if (item.GetIndex() != temp)
                    item.SetIndex(temp);
            }
        }

        private static CollumnInfo GetCollumnAttributes(CollumnInfo collumn)
        {
            collumn.CollumnAttribute = (DBCollumnAttribute)Attribute.GetCustomAttribute(collumn.Property, typeof(DBCollumnAttribute));
            collumn.ForeignAttribute = (DBCollumnForeignAttribute)Attribute.GetCustomAttribute(collumn.Property, typeof(DBCollumnForeignAttribute));
            collumn.AppendTableAttribute = (DBAppendTableAttribute)Attribute.GetCustomAttribute(collumn.Property, typeof(DBAppendTableAttribute));

            return collumn;
        }


        private static List<CollumnInfo> GetCollumns(Type t, DBCollumnForeignAttribute foreignKey, DBSealedCollumnAttribute[] append_collumns, string[] prefixes, DBCollumnForeignKeyAttribute[] foreignKeys)
        {
            List<CollumnInfo> r = new List<CollumnInfo>();

            if (t.BaseType != typeof(object))
            {
                r.AddRange(GetCollumns(t.BaseType, foreignKey,null,prefixes, foreignKeys));
            }

            if (foreignKey?.Sealed != true)
            {
                r.AddRange(t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                           BindingFlags.DeclaredOnly)
                    .Where(x =>
                    (Attribute.GetCustomAttribute(x, typeof(DBCollumnAttribute)) != null ||
                    Attribute.GetCustomAttribute(x, typeof(DBCollumnForeignAttribute)) != null ||
                    Attribute.GetCustomAttribute(x, typeof(DBAppendTableAttribute)) != null)
                    )
                    .Select(x => GetCollumn(x)).ToList());

                r.AddRange(t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                           BindingFlags.DeclaredOnly)
                    .Where(x =>
                    (Attribute.GetCustomAttribute(x, typeof(DBCollumnAttribute)) != null ||
                    Attribute.GetCustomAttribute(x, typeof(DBCollumnForeignAttribute)) != null ||
                    Attribute.GetCustomAttribute(x, typeof(DBAppendTableAttribute)) != null)
                    )
                    .Select(x => GetCollumn(x)).ToList());

                FillPrefixes(r, foreignKey?.ForeignPrefixes, prefixes, foreignKeys);

                foreach (var item in r)
                {
                    if (item.Type.IsGenericType && item.Type.GetGenericTypeDefinition() == typeof(List<>))
                        item.Type = item.Type.GetGenericArguments()[0];
                    else if(item.Type.IsGenericType && item.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        item.Type = item.Type.GetGenericArguments()[1];

                    if (item.Type.BaseType == typeof(Enum))
                        item.Type = typeof(Enum);

                    if (item.ForeignAttribute != null)
                    {
                        item.IsMigrationType = true;

                        item.MigrationTypeInfo = LoadType(item.Type, new List<object>(), foreignKey: item.ForeignAttribute, sealed_collumns: Attribute.GetCustomAttributes(item.Property, typeof(DBSealedCollumnAttribute)).Cast<DBSealedCollumnAttribute>().ToArray(), prefixes: prefixes);

                        item.MigrationTypeInfo.TableName = item.ForeignAttribute.Table;

                        foreach (DBAppendCollumnAttribute col in Attribute.GetCustomAttributes(item.Property, typeof(DBAppendCollumnAttribute)))
                        {
                            var c = new CollumnInfo()
                            {
                                AppendCollumnAttribute = col,
                                CollumnAttribute = new DBCollumnAttribute(col.Name) { Type = col.Type },
                                IsAppendCollumn = true,
                                IsMigrationType = false,
                                IsAppendTable = false,
                                Type = col.Type
                            };
                            c.Setter = (obj, val) =>
                            {
                                item.MigrationTypeInfo[obj, c.AppendCollumnAttribute.Name] = val;
                            };
                            c.Getter = (obj) =>
                            {
                                return item.MigrationTypeInfo[obj, c.AppendCollumnAttribute.Name];
                            };
                            item.MigrationTypeInfo.Collumns.Add(c);
                        }

                        FillPrefixes(item.MigrationTypeInfo.Collumns, foreignKey?.ForeignPrefixes, prefixes, foreignKeys);
                        item.MigrationTypeInfo.HaveId = item.MigrationTypeInfo.Collumns.Exists(x => x.CollumnAttribute?.Name == item.MigrationTypeInfo.IdKey || x.AppendCollumnAttribute?.Name == item.MigrationTypeInfo.IdKey);

                        if (item.MigrationTypeInfo.HaveId)
                        {
                            item.MigrationTypeInfo.IdCollumn = item.MigrationTypeInfo.Collumns.FirstOrDefault(x => x.CollumnAttribute?.Name == item.MigrationTypeInfo.IdKey || x.AppendCollumnAttribute?.Name == item.MigrationTypeInfo.IdKey);
                        }

                        if (item.MigrationTypeInfo.HaveForeign)
                        {
                            item.MigrationTypeInfo.ForeignKey = item.ForeignAttribute.Name;
                            item.MigrationTypeInfo.ForeignCollumn = item.MigrationTypeInfo.Collumns.FirstOrDefault(x => x.CollumnAttribute?.Name == item.ForeignAttribute.Name || x.AppendCollumnAttribute?.Name == item.ForeignAttribute.Name);
                        }
                    }
                    else if (item.AppendTableAttribute != null)
                    {
                        item.IsAppendTable = true;
                        item.MigrationTypeInfo = LoadType(item.Type, new List<object>(), sealed_collumns: Attribute.GetCustomAttributes(item.Property, typeof(DBSealedCollumnAttribute)).Cast<DBSealedCollumnAttribute>().ToArray(), prefixes: prefixes);
                    }
                }
            }

            if (append_collumns != null)
            {
                CollumnInfo collumn = null;
                foreach (var item in append_collumns)
                {
                    var prop = t.GetProperty(item.MemberName);
                    if (prop == null)
                    {
                        var field = t.GetField(item.MemberName);
                        if (field != null)
                            collumn = GetCollumn(field);
                    }
                    else
                    {
                        collumn = GetCollumn(prop);
                    }

                    collumn.CollumnAttribute = new DBCollumnAttribute(item.Name) { Type = item.Type };

                    r.Add(collumn);
                }
            }

            FillPrefixes(r, foreignKey?.ForeignPrefixes, prefixes, foreignKeys);

            foreach (var item in r)
            {
                var ignored = (DBMigrationIgnoreFieldsAttribute)Attribute.GetCustomAttribute(item.Property, typeof(DBMigrationIgnoreFieldsAttribute));
                if (ignored == null)
                    continue;

                item.MigrationTypeInfo.Collumns.RemoveAll(x => x.Property != null && ignored.NameArray.Contains(x.Property.Name));
            }

            return r;
        }

        private static void FillPrefixes(List<CollumnInfo> r, string[] foreignPrefixes, string[] prefixes, DBCollumnForeignKeyAttribute[] foreignKeys)
        {
            if (prefixes != null && prefixes.Length != 0)
            {
                if (foreignKeys != null)
                    foreach (var item in foreignKeys)
                    {
                        for (int i = 0; i < prefixes.Length; i++)
                        {
                            int p = i + 1;
                            item.DestinationCollumnName = item.DestinationCollumnName.Replace($"{{*Prefix{p}}}", prefixes[i]);
                            item.SourceCollumnName = item.SourceCollumnName.Replace($"{{*Prefix{p}}}", prefixes[i]);
                        }
                    }

                foreach (var item in r)
                {
                    for (int i = 0; i < prefixes.Length; i++)
                    {
                        int p = i + 1;
                        if (item.CollumnAttribute != null)
                        {
                            item.CollumnAttribute.Name = item.CollumnAttribute.Name.Replace($"{{*Prefix{p}}}", prefixes[i]);
                        }

                        if (item.AppendCollumnAttribute != null)
                        {
                            item.AppendCollumnAttribute.Name = item.AppendCollumnAttribute.Name.Replace($"{{*Prefix{p}}}", prefixes[i]);
                        }

                        if (item.ForeignAttribute != null)
                        {
                            item.ForeignAttribute.Name = item.ForeignAttribute.Name.Replace($"{{*Prefix{p}}}", prefixes[i]);
                            item.ForeignAttribute.Table = item.ForeignAttribute.Table.Replace($"{{*Prefix{p}}}", prefixes[i]);
                        }
                    }
                }
            }


            if (foreignPrefixes != null && foreignPrefixes.Length != 0)
            {
                if (foreignKeys != null)
                    foreach (var item in foreignKeys)
                    {
                        for (int i = 0; i < foreignPrefixes.Length; i++)
                        {
                            int p = i + 1;
                            item.DestinationCollumnName = item.DestinationCollumnName.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                            item.SourceCollumnName = item.SourceCollumnName.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                        }
                    }

                foreach (var item in r)
                {
                    for (int i = 0; i < foreignPrefixes.Length; i++)
                    {
                        int p = i + 1;
                        if (item.CollumnAttribute != null)
                        {
                            item.CollumnAttribute.Name = item.CollumnAttribute.Name.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                        }

                        if (item.AppendCollumnAttribute != null)
                        {
                            item.AppendCollumnAttribute.Name = item.AppendCollumnAttribute.Name.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                        }

                        if (item.ForeignAttribute != null)
                        {
                            item.ForeignAttribute.Name = item.ForeignAttribute.Name.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                            item.ForeignAttribute.Table = item.ForeignAttribute.Table.Replace($"{{*ForeignPrefix{p}}}", foreignPrefixes[i]);
                        }
                    }
                }
            }
        }

        public object this[object item, string name]
        {
            get
            {
                object o = null;
                if (!AppendedValues.TryGetValue(new KeyValuePair<string, object>(name, item), out o))
                {
                    o = Activator.CreateInstance(Collumns.FirstOrDefault(x => x.AppendCollumnAttribute?.Name == name).Type);
                }
                return o;
            }
            set
            {
                var id = new KeyValuePair<string, object>(name, item);
                if (AppendedValues.ContainsKey(id))
                    AppendedValues[id] = value;
                else
                    AppendedValues.Add(id, value);
            }
        }

        public override string ToString()
        {
            return TableName;
        }
        
        private static CollumnInfo GetCollumn(PropertyInfo x)
        {
            return GetCollumnAttributes(new CollumnInfo
            {
                Property = x,
                Type = x.PropertyType,
                Getter = new Func<object, object>((object obj) => { return GetValue(x, obj); }),
                Setter = new Action<object, object>((object obj, object val) => { x.SetValue(obj, val); })
            });
        }

        private static CollumnInfo GetCollumn(FieldInfo x)
        {
            return GetCollumnAttributes(new CollumnInfo
            {
                Property = x,
                Type = x.FieldType,
                Getter = new Func<object, object>((object obj) => { return GetValue(x, obj); }),
                Setter = new Action<object, object>((object obj, object val) => { x.SetValue(obj, val); })
            });
        }

        private static object GetValue(PropertyInfo property, object obj)
        {
            if (property.PropertyType.IsEnum)
                return Convert.ToInt32(property.GetValue(obj));
            return property.GetValue(obj);
        }

        private static object GetValue(FieldInfo property, object obj)
        {
            if (property.FieldType.IsEnum)
                return Convert.ToInt32(property.GetValue(obj));
            return property.GetValue(obj);
        }
    }
}
