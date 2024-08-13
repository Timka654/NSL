using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

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
}
