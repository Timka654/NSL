using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.Host;

namespace NSL.Database.EntityFramework.Filter.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            DbContextOptionsBuilder b = new DbContextOptionsBuilder();
            //b.UseInMemoryDatabase("dev");
            b.UseNpgsql("Host=localhost;Port=5432;Database=devdb_0;Username=postgres;Password=postgres");

            var context = new TestDbContext(b.Options);

            if (!await context.Tests.AnyAsync())
                context.Tests.AddRange(
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabb" },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aAbb" },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aaBb", NullCheckDate = DateTime.UtcNow },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabB" },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aABb", NullCheckDate = DateTime.UtcNow },
                    new TestEntityModel() { Id = Guid.NewGuid(), Content = "aabba" }
                    );

            await context.SaveChangesAsync();

            var builder = EntityFilterBuilder.Create()
                .CreateFilterBlock(b => b
                    .AddProperty(nameof(TestEntityModel.NullCheckDate), Enums.CompareType.NotEquals, null)
                    //.AddFilter(nameof(TestEntityModel.Content), Enums.CompareType.ContainsCase, "bb")
                )
                .AddOrderProperty(nameof(TestEntityModel.NullCheckDate));


            var result = await context.Tests
                .Filter(builder.GetFilter())
                .ToDataResultAsync();
        }
    }
}
