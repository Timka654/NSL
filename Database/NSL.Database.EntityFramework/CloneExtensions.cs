using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Database.EntityFramework
{
    public static class CloneExtensions
    {
        private static async Task LoadUnattachedAsync(DbContext dbContext, object value, IReadOnlyList<RuntimeProperty> p = null)
        {
            var entry = dbContext.Entry(value);

            foreach (var collectionItem in entry.Collections)
            {
                await collectionItem.LoadAsync();

                var key = (collectionItem.Metadata as RuntimeNavigation).ForeignKey;

                foreach (var item in collectionItem.CurrentValue)
                {
                    await LoadUnattachedAsync(dbContext, item, key.Properties);
                }
            }

            entry.State = EntityState.Detached;

            List<string> changedKeys = new();

            if (p != null)
            {
                foreach (var item in p)
                {
                    changedKeys.Add(item.Name);

                    var prop = value.GetType().GetProperty(item.Name);

                    prop.SetValue(value, GetDefaultValue(prop.PropertyType));
                }
            }

            var keys = entry.Metadata.GetDeclaredKeys();

            foreach (var key in keys)
            {
                if (key.Properties.Any(x => changedKeys.Contains(x.Name)))
                    continue;

                foreach (var item in key.Properties)
                {
                    var prop = value.GetType().GetProperty(item.Name);

                    prop.SetValue(value, GetDefaultValue(prop.PropertyType));
                }
            }
        }

        /// <summary>
        /// Return entity unattached from dbContext with load all collections values and set default values on foreign and primary keys properties
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<TEntity> LoadUnattachedAsync<TEntity>(this DbContext dbContext, params object[] key)
            where TEntity : class
        {
            var value = await dbContext.Set<TEntity>().FindAsync(key);

            if (value == default)
                return null;

            await LoadUnattachedAsync(dbContext, value);

            return value;
        }

        /// <summary>
        /// Return entity unattached from dbContext with load all collections values and set default values on foreign and primary keys properties
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<bool> CloneEntityAsync<TEntity>(this DbContext dbContext, params object[] key)
            where TEntity : class
        {
            var value = await dbContext.LoadUnattachedAsync<TEntity>(key);

            if (value == null)
                return false;

            var newEntry = dbContext.Add(value);

            return newEntry.State == EntityState.Added;
        }

        private static Dictionary<Type, object> defaultValuesTemp = new();

        private static object GetDefaultValue(Type type)
        {
            // Validate parameters.
            if (type == null) throw new ArgumentNullException("type");

            if (defaultValuesTemp.TryGetValue(type, out var result))
                return result;

            // We want an Func<object> which returns the default.
            // Create that expression here.
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
            result = e.Compile()();


            defaultValuesTemp.TryAdd(type, result);

            return result;
        }
    }
}
