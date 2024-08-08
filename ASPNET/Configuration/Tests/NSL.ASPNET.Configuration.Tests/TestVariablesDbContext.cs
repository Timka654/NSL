using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NSL.ASPNET.Configuration;

namespace NSL.ASPNET.MemoryLogger.Tests
{
    public class TestVariablesDbContext : DbContext
    {
        public const string DEVConStr = "Host=localhost;Port=5432;Database=db_config;Username=postgres;Password=postgres";

        public TestVariablesDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestVariableModel> Variables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var variableEntity = modelBuilder.Entity<TestVariableModel>();

            variableEntity.DefaultConfigure();

            variableEntity.HasData(new TestVariableModel() { Name = "dv0", Value = "1", UpdateTime = new DateTime(2024, 08, 09) });

            base.OnModelCreating(modelBuilder);
        }

        public static TestVariablesDbContext Create()
            => new TestVariablesDbContext(new DbContextOptionsBuilder<TestVariablesDbContext>()
                .UseNpgsql(DEVConStr)
                .Options);
    }

    public class TestVariablesDbContextDesign : IDesignTimeDbContextFactory<TestVariablesDbContext>
    {
        public TestVariablesDbContext CreateDbContext(string[] args)
            => TestVariablesDbContext.Create();
    }
}
