using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using NSL.Database.EntityFramework.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NSL.Database.EntityFramework
{
    public static class EntityExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ignoreTypes">excepted for processing</param>
        public static void IgnoreAllExceptIncluded(this ModelBuilder builder, params IMutableEntityType[] ignoreTypes)
        {
            var types = builder.Model.GetEntityTypes().Except(ignoreTypes).ToArray();

            foreach (var entityType in types)
            {
                var type = entityType.ClrType;

                if (type.GetCustomAttribute<EntityExcludeModelAttribute>(false) != null)
                {
                    builder.Ignore(entityType.ClrType);
                    continue;
                }

                var p = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToArray();

                var incp = p.Where(x => x.GetCustomAttribute<EntityIncludeAttribute>() != null).ToArray();

                var exceptedp = p.Except(incp).ToArray();

                if (exceptedp.Length == p.Length)
                {
                    builder.Ignore(entityType.ClrType);
                    continue;
                }

                foreach (var item in exceptedp)
                {
                    builder.Entity(item.ReflectedType).Ignore(item.Name);
                }
            }
        }

        public static void IgnoreAllExceptIncluded<T>(this EntityTypeBuilder<T> builder)
            where T : class
        {
            var type = typeof(T);

            var p = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToArray();

            var incp = p.Where(x => x.GetCustomAttribute<EntityIncludeAttribute>() != null).ToArray();

            var exceptedp = p.Except(incp).ToArray();

            foreach (var item in exceptedp)
            {
                builder.Ignore(item.Name);
            }
        }
    }
}
