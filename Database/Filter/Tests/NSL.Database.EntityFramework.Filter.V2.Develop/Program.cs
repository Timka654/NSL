using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Builders;
using NSL.Database.EntityFramework.Filter.V2.Develop.Data;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using System.Text.Json;

namespace NSL.Database.EntityFramework.Filter.V2.Develop
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var context = await TestDbContext.Create(DateTime.UtcNow);

            var f = FilteredQueryBuilder.Create<FullFilteredQueryModel, TestEntityModel>()
                .WithFilter(fb => fb
                    .Or(fn => fn.Where(b => b.Number, V2.Enums.FilterOperator.GreaterThan, 1.ToString()))
                )
                .Select(
                    "Id",
                    "NullFloatNumber",
                    "Number",
                    $"{nameof(TestEntityModel.RelTests)}.Type"
                )
                .Build();

            var rquery = context.Tests
                .Include(x => x.RelTests)
                .WithQueryModel(f)
                .SelectDynamic(f.Properties);

            var output = await rquery.ToArrayAsync();

            var jout = JsonSerializer.Serialize(output, JsonSerializerOptions.Web);


            //var fout = FilteredQueryBuilder.Create<FullFilteredQueryModel, TestEntityModel>()
            //    .WithFilter(fb => fb
            //        .Or(fn => fn.Where(b => b.Number, V2.Enums.FilterOperator.GreaterThan, 1.ToString()))
            //    )
            //    .Select(
            //        x => x.Id,
            //        x => x.NullFloatNumber,
            //        x => x.Number,
            //        x=>x.
            //    )
            //    .Build();


            //var builder = EntityFilterBuilder.Create()
            //    .CreateFilterBlock(b => b
            //        //.AddProperty(nameof(TestEntityModel.NullCheckDate), Enums.CompareType.NotEquals, null)
            //        .AddProperty(nameof(TestEntityModel.Content), Enums.CompareType.ContainsIgnoreCase, "bb%", false)
            //        //.AddProperty(nameof(TestEntityModel.RelTests), Enums.CompareType.ContainsCollection, b2=> b2.AddProperty(nameof(RelTestEntityModel.Type), Enums.CompareType.Equals, 1))
            //    )
            //    .AddOrderProperty(nameof(TestEntityModel.NullCheckDate));

            //var rquery = context.Tests
            //    .Include(x => x.RelTests)
            //    .WithFilter(builder.GetFilter());

            //var result = await rquery
            //    .ToDataResultAsync();
        }
    }
}
