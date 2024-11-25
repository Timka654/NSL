using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.Host;

namespace NSL.Database.EntityFramework.Filter.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestEntityModel> Tests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFilter();

            base.OnModelCreating(modelBuilder);
        }
    }
}
