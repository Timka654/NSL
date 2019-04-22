using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;
using System.Data.Common;
using DBEngine.DBMigrator.ConfigurationAttributes;

namespace DBEngine.DBMigrator
{
    /*
     * Entity Framework курильщика
    */
    public class MigrationManager<T, TSelectAttribute>
        where T : DbConnection
        where TSelectAttribute : DBAutoMigrationAttribute
    {
        private static DbConnectionPool<T> connPool;

        public static void UpdateDb(DbConnectionPool<T> connect_pool)
        {
            connPool = connect_pool;

            var classes = Assembly.GetCallingAssembly().GetTypes()
                .Select(x => new { type = x, attr = x.GetCustomAttribute<TSelectAttribute>() })
                .Where(x => x.attr != null)
                .OrderBy(x => x.attr.Order != 0)
                .OrderBy(x => x.attr.Order)
                .ToList();

            foreach (var item in classes)
            {
                typeof(MigrationManager<T, TSelectAttribute>).GetMethod("UpdateDbGenericConfig", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(item.type).Invoke(null, null);
            }
        }

        private static void UpdateDbGenericConfig<TQ>() where TQ : DBIdentityEntity
        {
            var inst = Activator.CreateInstance<TQ>();
            
            var type = MigrationTypeInfo.LoadType(typeof(TQ), inst is DBDataEntity dde? dde.GetData() : new List<TQ>());
            MigrationTypeInfo.FillIds(type);
            UpdateDbGeneric<TQ>(type, 0);
        }


        private static void SelectExistCollumns(MigrationTypeInfo type)
        {
            DBCommand cm = connPool.GetQueryCommand($@"Select c.name, t.name, c.is_nullable from sys.columns as c
            left join sys.types as t on t.system_type_id = c.system_type_id and t.name not like 'sysname'
            where c.object_id = OBJECT_ID('dbo.{type.TableName}')");
            cm.ExecuteAndRead((reader) =>
            {
                type.ExistDbCollumns.Add(CollumnInfo.GetSqlCollumnInfo(reader.GetString(0), "[" + reader.GetString(1) + "]", reader.GetBoolean(2)));
            });
            cm.CloseConnection();
        }

        private static List<CollumnInfo> SelectNewCollumns(MigrationTypeInfo type)
        {
            List<CollumnInfo> _result = new List<CollumnInfo>();
            foreach (var item in type.Collumns)
            {
                if (item.CollumnAttribute != null)
                {
                    var sqlType = DBCollumnType.GetSQLTypeData(item).Type;
                    if(sqlType.Contains('('))
                        _result.Add(CollumnInfo.GetSqlCollumnInfo(item.CollumnAttribute.Name, sqlType.Substring(0, sqlType.IndexOf("(")), IsOfNullableType(item.CollumnAttribute.Type ?? item.Type)));
                    else
                        _result.Add(CollumnInfo.GetSqlCollumnInfo(item.CollumnAttribute.Name, sqlType, IsOfNullableType(item.CollumnAttribute.Type ?? item.Type)));
                }
                else if (item.IsAppendTable)
                    _result.AddRange(SelectNewCollumns(item.MigrationTypeInfo));
            }

            return _result;
        }

         static bool IsOfNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static void UpdateDbGeneric<TQ>(MigrationTypeInfo type, object foreignValue)
        {
            if (type.Collumns.Count == 0)
                return;
            SelectExistCollumns(type);

            type.DbCollumns = SelectNewCollumns(type);
            DBCommand cm = connPool.GetQueryCommand();

            bool mustMigrate = false;
            
            var filtred = type.ExistDbCollumns.Where(x => type.DbCollumns.Exists(y => y.CollumnAttribute?.Name == x.CollumnAttribute.Name)).ToList();

            if (filtred.Count != type.ExistDbCollumns.Count || filtred.Count != type.DbCollumns.Count)
                mustMigrate = true;

            type.ExistDbCollumns = filtred;
            
            string migrateCollumns = string.Join(",", type.ExistDbCollumns.Select(x => $"{x.CollumnAttribute.Name}"));
            
            List<string> existCols = type.ExistDbCollumns.Select((e) => {
                var t1 = type.DbCollumns.FirstOrDefault(x => x.CollumnAttribute?.Name == e.CollumnAttribute.Name);
                if (t1.SqlType.Type == e.SqlType.Type)
                    return t1.CollumnAttribute.Name;
                mustMigrate = true;
                return $"CAST({t1.CollumnAttribute.Name} as {e.SqlType})";
            }).ToList();

            string copyQuery = type.ExistDbCollumns.Count == 0 || !mustMigrate ? "" : $"INSERT INTO dbo.tmp_{type.TableName} ({migrateCollumns}) \r\n Select {string.Join(",", existCols)} from dbo.{type.TableName}";

            string query = !mustMigrate ? "" : $@"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES
                 WHERE TABLE_SCHEMA = 'dbo'
                 AND TABLE_NAME = 'tmp_{type.TableName}'))
                BEGIN
                    Drop table dbo.tmp_{type.TableName}
                END" + 
                "\r\n\r\n\r\n\r\n" +
                GetCreationQuery(type) +
                "\r\n\r\n\r\n\r\n" +
                copyQuery +
                "\r\n\r\n\r\n" +
                $@"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES
                 WHERE TABLE_SCHEMA = 'dbo'
                 AND TABLE_NAME = '{type.TableName}'))
                BEGIN
                    Drop table dbo.{type.TableName}
                END" +
                "\r\n\r\n\r\n" +
                $"EXEC sp_rename 'tmp_{type.TableName}', '{type.TableName}'" +
                "\r\n\r\n\r\n";

            var insert_collumn_fragment = GetInsertCollumns(type);
            if (type.HaveForeign && type.ForeignCollumn == null)
            {
                insert_collumn_fragment.Add($"[{type.ForeignKey}]");
            }

            cm.Query = query;
            if (!string.IsNullOrEmpty(query))
                cm.Execute();
            query = "";


            foreach (var item in type.Values)
            {
                List<string> update_fragment = new List<string>();
                List<string> insert_fragment = new List<string>();

                if (type.HaveForeign && type.ForeignCollumn == null)
                {
                    update_fragment.Add($"{type.ForeignKey} = {foreignValue}");
                    insert_fragment.Add(foreignValue.ToString().Replace(',', '.'));
                }
                else if (type.HaveForeign)
                {
                    type.ForeignCollumn.Setter(item, foreignValue);
                }
                update_fragment.AddRange(GetUpdateValues(type, item));

                insert_fragment.AddRange(GetInsertValues(type, item));

                if (update_fragment.Count == 0 || insert_collumn_fragment.Count == 0 || insert_fragment.Count == 0)
                    continue;

                string where = GetWhereSegment(type, item, foreignValue);

                query += $"IF ((Select Count(*) from dbo.{type.TableName} where {where} ) = 1) BEGIN \r\n " +
                    $"Update dbo.{type.TableName} set {string.Join(",", update_fragment.ToArray())} \r\n " +
                    $"where {where}\r\n " +
                    $"END\r\nElSE\r\nBEGIN\r\n " +
                    $"Insert into dbo.{type.TableName} ({string.Join(",", insert_collumn_fragment.ToArray())}) Values ({string.Join(",", insert_fragment.ToArray())})\r\n " +
                    $"END\r\n ";
            }
            cm.Query = query;
            if (!string.IsNullOrEmpty(cm.Query))
                cm.Execute();
            cm.CloseConnection();

            foreach (var item in type.Collumns)
            {
                ForeignFill(item, type, null);
            }

        }

        private static void ForeignFill(CollumnInfo collumn, MigrationTypeInfo type, object foreignKeyValue)
        {
            if (!collumn.IsAppendTable && collumn.ForeignAttribute == null)
                return;
            bool nullKey = foreignKeyValue == null;
            foreach (var item in type.Values)
            {

                if (type.IdCollumn != null && nullKey)
                    foreignKeyValue = type.IdCollumn.Getter(item);


                if (collumn.IsAppendTable)
                {
                    collumn.MigrationTypeInfo.Values = new[] { collumn.Getter(item) };
                    foreach (var col in collumn.MigrationTypeInfo.Collumns)
                    {
                        ForeignFill(col, collumn.MigrationTypeInfo, foreignKeyValue);
                    }
                    continue;
                }

                if (collumn.ForeignAttribute == null)
                    continue;


                collumn.MigrationTypeInfo.Values = (IEnumerable)collumn.Getter(item);

                if (collumn.MigrationTypeInfo.SourceKeys != null && collumn.MigrationTypeInfo.Values != null)
                    foreach (var sourceKey in collumn.MigrationTypeInfo.SourceKeys)
                    {
                        sourceKey.SourceValue = type.Collumns.Find(x => x.CollumnAttribute.Name == sourceKey.SourceCollumnName).Getter(item);

                        foreach (var value in collumn.MigrationTypeInfo.Values)
                        {
                            collumn.MigrationTypeInfo.Collumns.Find(x => x.CollumnAttribute.Name == sourceKey.DestinationCollumnName).Setter(value, sourceKey.SourceValue);
                        }
                    }

                typeof(MigrationManager<T, TSelectAttribute>).GetMethod("UpdateDbGeneric", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(collumn.Type).Invoke(null, new object[] { collumn.MigrationTypeInfo, foreignKeyValue });
            }

            foreach (var item in type.Values)
            {
                return;
            }

            if (collumn.IsAppendTable)
            {
                foreach (var col in collumn.MigrationTypeInfo.Collumns)
                {
                    ForeignFill(col, collumn.MigrationTypeInfo, foreignKeyValue);
                }
                return;
            }

            if (collumn.ForeignAttribute == null)
                return;



            if (collumn.MigrationTypeInfo.SourceKeys != null && collumn.MigrationTypeInfo.Values != null)
                foreach (var sourceKey in collumn.MigrationTypeInfo.SourceKeys)
                {
                    //sourceKey.SourceValue = type.Collumns.Find(x => x.collumn_attribute.Name == sourceKey.SourceCollumnName).getter(item);

                    foreach (var value in collumn.MigrationTypeInfo.Values)
                    {
                        collumn.MigrationTypeInfo.Collumns.Find(x => x.CollumnAttribute.Name == sourceKey.DestinationCollumnName).Setter(value, sourceKey.SourceValue);
                    }
                }

            typeof(MigrationManager<T, TSelectAttribute>).GetMethod("UpdateDbGeneric", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(collumn.Type).Invoke(null, new object[] { collumn.MigrationTypeInfo, foreignKeyValue });
        }

        #region Utils

        private static string GetWhereSegment(MigrationTypeInfo t, object item, object foreignValue)
        {
            List<string> where_fragment = new List<string>();

            if (t.HaveId)
                where_fragment.Add($"{t.IdKey} = {t.IdCollumn.SqlType.Getter(t.IdCollumn.Getter(item))}");
            if (t.HaveForeign)
                where_fragment.Add($"{t.ForeignKey} = {foreignValue}");

            return string.Join(" and ", where_fragment.ToArray());
        }

        private static List<string> GetInsertCollumns(MigrationTypeInfo t)
        {
            List<string> insert_fragment = new List<string>();

            foreach (var collumn in t.Collumns)
            {
                if (!collumn.IsAppendTable && !collumn.IsMigrationType)
                {
                    if (collumn.CollumnAttribute.Type == null)
                        collumn.CollumnAttribute.Type = collumn.Type;

                    insert_fragment.Add($"[{collumn.CollumnAttribute.Name}]");
                }
                else if (collumn.IsAppendTable)
                    insert_fragment.AddRange(GetInsertCollumns(collumn.MigrationTypeInfo));
            }

            return insert_fragment;
        }

        private static List<string> GetCreateCollumns(MigrationTypeInfo t)
        {
            List<string> create_fragment = new List<string>();

            foreach (var collumn in t.Collumns)
            {
                if (collumn.CollumnAttribute != null)
                {
                    if (collumn.CollumnAttribute.Type == null)
                        collumn.CollumnAttribute.Type = collumn.Type;

                    create_fragment.Add($"[{collumn.CollumnAttribute.Name}] {collumn.SqlType.Type} {collumn.SqlType.NullSegment} {(t.ExistDbCollumns.Exists(x => x.CollumnAttribute.Name == collumn.CollumnAttribute.Name) ? "" : "DEFAULT " + collumn.SqlType.DefaultValue())}");
                }
                else if (collumn.IsAppendTable)
                    create_fragment.AddRange(GetCreateCollumns(collumn.MigrationTypeInfo));
            }

            return create_fragment;
        }

        private static List<string> GetUpdateValues(MigrationTypeInfo t, object item)
        {
            List<string> update_fragment = new List<string>();

            foreach (var collumn in t.Collumns)
            {
                if (collumn.IsAppendCollumn && collumn.AppendCollumnAttribute.AutoIncrement)
                    t[item, collumn.CollumnAttribute.Name] = collumn.AppendCollumnAttribute.Increment();

                if (collumn.SqlType != null)
                    update_fragment.Add(collumn.CollumnAttribute.Name + " = " + collumn.SqlType.Getter(collumn.Getter(item)));
                else if (collumn.IsAppendTable)
                    update_fragment.AddRange(GetUpdateValues(collumn.MigrationTypeInfo, collumn.Getter(item)));
            }

            return update_fragment;

        }

        private static List<string> GetInsertValues(MigrationTypeInfo t, object item)
        {
            List<string> insert_fragment = new List<string>();
            if (item == null)
                return insert_fragment;

            foreach (var collumn in t.Collumns)
            {
                if (collumn.SqlType != null)
                    insert_fragment.Add(collumn.SqlType.Getter(collumn.Getter(item)));
                else if (collumn.IsAppendTable)
                    insert_fragment.AddRange(GetInsertValues(collumn.MigrationTypeInfo, collumn.Getter(item)));

            }

            return insert_fragment;
        }

        private static string GetCreationQuery(MigrationTypeInfo t)
        {
            List<string> create_fragment = GetCreateCollumns(t);

            if (t.HaveForeign && t.ForeignCollumn == null)
                create_fragment.Add($"[{t.ForeignKey}] [int] NOT NULL");

            return $"Create Table [dbo].[tmp_{t.TableName}] (\r\n {string.Join(",\r\n", create_fragment.ToArray())} \r\n)";
        }

        #endregion
    }
}