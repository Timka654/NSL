
using Microsoft.EntityFrameworkCore;

namespace NSL.Generators.SelectTypeGenerator.EF.Tests
{
    public class DevDbContext : DbContext
    {
        public DevDbContext(DbContextOptions options) : base(options)
        {
        }

        protected DevDbContext()
        {
        }

        public DbSet<Dev1Model> Dev1 { get; set; }
        public DbSet<Dev1Model> Dev2 { get; set; }
    }
}
