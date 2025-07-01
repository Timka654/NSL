using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSL.Database.EntityFramework.Filter.Host;

namespace NSL.Database.EntityFramework.Filter.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var b = new DbContextOptionsBuilder<TestDbContext>();
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
                    //.AddProperty(nameof(TestEntityModel.NullCheckDate), Enums.CompareType.NotEquals, null)
                    .AddProperty(nameof(TestEntityModel.Content), Enums.CompareType.ContainsIgnoreCase, "bb%", false)
                    //.AddProperty(nameof(TestEntityModel.RelTests), Enums.CompareType.ContainsCollection, b2=> b2.AddProperty(nameof(RelTestEntityModel.Type), Enums.CompareType.Equals, 1))
                )
                .AddOrderProperty(nameof(TestEntityModel.NullCheckDate));

            var rquery = context.Tests
                .Include(x => x.RelTests)
                .Filter(builder.GetFilter());

            var result = await rquery
                .ToDataResultAsync();
        }
    }
}
