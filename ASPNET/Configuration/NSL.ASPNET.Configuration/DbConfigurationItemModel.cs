using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.ASPNET.Configuration
{
    /// <summary>
    /// For default entity configuration call
    /// - <see cref="DbConfigurationItemModelEntity.DefaultConfigure{TEntity}(EntityTypeBuilder{TEntity})"/> 
    /// - <see cref="DbConfigurationItemModelEntity.DefaultConfigure{TEntity}(ModelBuilder)"/>
    /// </summary>
    public class DbConfigurationItemModel
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public DateTime UpdateTime { get; set; }

        public void UpdateValue(string value)
        {
            Value = value;
            UpdateTime = DateTime.UtcNow;
        }

    }

    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="set"></param>
        /// <param name="item"></param>
        /// <returns>have any for update</returns>
        public static async Task<bool> TryUpdateValueAsync<TItem>(this DbSet<TItem> set, TItem item)
            where TItem : DbConfigurationItemModel
        {
            return await set
                    .Where(x => x.Name == item.Name)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(x => x.UpdateTime, DateTime.UtcNow)
                        .SetProperty(x => x.Value, item.Value)) > 0;
        }
    }
}
