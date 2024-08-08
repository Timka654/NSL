using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Configuration
{
    public class DbConfigurationOptions<TContext>
        where TContext : DbContext
    {
        internal DbContextOptions<TContext> DBOptions { get; set; }

        internal Func<DbContextOptions<TContext>, TContext> DBContextGetter { get; private set; }

        internal DbConfigurationAllowActionDelegate AllowCreate { get; private set; } = (key) => Task.FromResult(false);

        internal DbConfigurationAllowActionDelegate AllowUpdate { get; private set; } = (key) => Task.FromResult(false);

        internal TimeSpan? CheckUpdatesDelay { get; private set; }

        /// <summary>
        /// Db context options build action
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public DbConfigurationOptions<TContext> WithDbContextOptions(Action<DbContextOptionsBuilder<TContext>> build)
        {
            var builder = new DbContextOptionsBuilder<TContext>();

            build(builder);

            DBOptions = builder.Options;

            return this;
        }

        /// <summary>
        /// Db context getter(required)
        /// </summary>
        /// <param name="contextGetter"></param>
        /// <returns></returns>
        public DbConfigurationOptions<TContext> WithDbContextGetter(Func<DbContextOptions<TContext>, TContext> contextGetter)
        {
            DBContextGetter = contextGetter;

            return this;
        }

        /// <summary>
        /// default: off
        /// </summary>
        public DbConfigurationOptions<TContext> WithAllowCreate(DbConfigurationAllowActionDelegate validator)
        {
            AllowCreate = validator;

            return this;
        }

        /// <summary>
        /// default: off
        /// </summary>
        public DbConfigurationOptions<TContext> WithAllowUpdate(DbConfigurationAllowActionDelegate validator)
        {
            AllowUpdate = validator;

            return this;
        }

        /// <summary>
        /// default: none
        /// </summary>
        public DbConfigurationOptions<TContext> WithReloadDelay(TimeSpan? checkUpdatesDelay)
        {
            CheckUpdatesDelay = checkUpdatesDelay;

            return this;
        }
    }
}
