using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace NSL.ASPNET.Configuration
{
    public static class DbConfigurationStartupExtensions
    {
        public static ConfigurationManager AddDbConfiguration<TContext, TEntity>(this ConfigurationManager configurationManager, Action<DbConfigurationOptions<TContext>> build)
        where TContext : DbContext
        where TEntity : DbConfigurationItemModel, new()
        {
            configurationManager.Add<DbConfigurationSource<TContext, TEntity>>(c => c.WithOptions(build));

            return configurationManager;
        }
    }
}
