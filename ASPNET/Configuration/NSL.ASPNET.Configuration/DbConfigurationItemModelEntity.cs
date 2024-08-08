using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NSL.ASPNET.Configuration
{
    public static class DbConfigurationItemModelEntity
    {
        public static void DefaultConfigure<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : DbConfigurationItemModel
        {
            builder.HasKey(x => x.Name);
        }
        public static void DefaultConfigure<TEntity>(this ModelBuilder builder)
        where TEntity : DbConfigurationItemModel
        {
            builder.Entity<TEntity>().DefaultConfigure();
        }
    }
}
