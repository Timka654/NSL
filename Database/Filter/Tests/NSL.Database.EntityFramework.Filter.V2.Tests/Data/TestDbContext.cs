using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Host;

namespace NSL.Database.EntityFramework.Filter.V2.Tests.Data
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestEntityModel> Tests { get; set; }

        public DbSet<RelTestEntityModel> RelTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFilterV2(this);

            base.OnModelCreating(modelBuilder);
        }

        public static async Task<TestDbContext> Create(DateTime? now, int dataSize = 6)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
            var b = new DbContextOptionsBuilder<TestDbContext>();
            b.UseNpgsql("Host=localhost;Port=5432;Database=devdb_0;Username=postgres;Password=postgres");

            var context = new TestDbContext(b.Options);


            if (dataSize > 0) // Only clear if we intend to add data
            {
                await context.RelTests.ExecuteDeleteAsync();
                await context.Tests.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
            }

            now ??= DateTime.UtcNow;

            if (dataSize > 0 && !await context.Tests.AnyAsync())
            {
                if (dataSize == 6) // Original data for correctness tests
                {
                    TestEntityModel[] tests = [
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabb", NullContent = "1", BooleanValue = true, NullableBooleanValue = true, CheckDate = now.Value.AddDays(-1), Enum = TestEnum.First, NullEnum = TestEnum.Second, FloatNumber = 1, NullFloatNumber = 1, Number = 1, NullNumber = 2 },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aAbb", NullContent = "1", BooleanValue = true, NullableBooleanValue = false, CheckDate = now.Value.AddDays(-1), Enum = TestEnum.First, NullEnum = TestEnum.Second, FloatNumber = 2, NullFloatNumber = 3, Number = 2, NullNumber = 3 },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aaBb", BooleanValue = true,  CheckDate = now.Value.AddDays(1), NullCheckDate = now, Enum = TestEnum.Second, FloatNumber = 2, Number = 3 },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabB", BooleanValue = false, CheckDate = now.Value.AddDays(1), Enum = TestEnum.Second },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aABb", BooleanValue = false,  CheckDate = now.Value, NullCheckDate = now, Enum = TestEnum.Second },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabba", BooleanValue = false, CheckDate = now.Value.AddDays(1), Enum = TestEnum.Third }
                    ];

                    context.Tests.AddRange(tests);
                    context.RelTests.AddRange(
                        new RelTestEntityModel() { Id = Guid.NewGuid(), Content = "rel_1", Type = 10, TestId = tests.First().Id },
                        new RelTestEntityModel() { Id = Guid.NewGuid(), Content = "rel_2", Type = 20, TestId = tests.First().Id },
                        new RelTestEntityModel() { Id = Guid.NewGuid(), Content = "rel_3", Type = 30, TestId = tests.Skip(2).First().Id }
                        );
                }
                else // Large data generation for performance tests
                {
                    var random = new Random();
                    var tests = new List<TestEntityModel>();
                    for (int i = 0; i < dataSize; i++)
                    {
                        tests.Add(new TestEntityModel
                        {
                            Id = Guid.NewGuid(),
                            Content = $"content_{i % 100}",
                            Number = i,
                            BooleanValue = i % 2 == 0,
                            CheckDate = now.Value.AddMinutes(i)
                        });
                    }
                    await context.AddRangeAsync(tests);
                }

                await context.SaveChangesAsync();
            }

            return context;
        }
    }
}
