using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Configuration
{
    public delegate Task<bool> DbConfigurationAllowActionDelegate(string key);

    public class DbConfigurationSource<TContext, TEntity> : IConfigurationSource
        where TContext : DbContext
        where TEntity : DbConfigurationItemModel, new()
    {
        DbConfigurationOptions<TContext> options = new DbConfigurationOptions<TContext>();

        public DbConfigurationSource<TContext, TEntity> WithOptions(Action<DbConfigurationOptions<TContext>> build)
        {
            build(options);

            return this;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(options.DBContextGetter, nameof(DbConfigurationOptions<TContext>.DBContextGetter));

            return new DbConfigurationProvider<TContext, TEntity>(options);
        }
    }
}
