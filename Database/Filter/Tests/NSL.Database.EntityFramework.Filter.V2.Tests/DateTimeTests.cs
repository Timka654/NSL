using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class DateTimeTests
    {
        TestDbContext dbContext;

        DateTime now;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            now = DateTime.UtcNow;
            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
            dbContext = await TestDbContext.Create(now);
        }

        [Test]
        public async Task Test_Filter_DateTime_Null_Equals()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.NullCheckDate),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = null,
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            Assert.That(output.Length, Is.EqualTo(4));
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.Where(x => x.NullCheckDate == null).CountAsync()));
        }

        [Test]
        public async Task Test_Filter_DateTime_Null_NoEquals()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.NullCheckDate),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = null,
                                Not = true
                            }
                        }
                    }
                }
            };
            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            Assert.That(output.Length, Is.EqualTo(2));
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.Where(x => x.NullCheckDate != null).CountAsync()));
        }

        [Test]
        public async Task Test_Filter_DateTime_Now_Equals()
        {
            FilterNode fn = new FilterNode()
            {
                Logic = FilterLogic.And,
                Nodes = new List<FilterNode>()
                {
                    new FilterNode()
                    {
                        Logic = FilterLogic.Or,
                        Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.CheckDate),
                                Type = V2.Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = now.ToString("o")
                            }
                        }
                    }
                }
            };

            var rquery = dbContext.Tests
                .WithFilter(fn);

            var output = await rquery.ToArrayAsync();

            var t = await dbContext.Tests.Where(x => x.CheckDate == now).ToArrayAsync();

            Assert.That(output.Length, Is.EqualTo(1));
            Assert.That(output.Length, Is.EqualTo(t.Count()));
        }

        [Test]
        public async Task Test_Filter_DateTime_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.CheckDate), Type = V2.Enums.FilterOperator.GreaterThan, Value = now.ToString("o") }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.CheckDate > now).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_DateTime_LessThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.CheckDate), Type = V2.Enums.FilterOperator.LessThan, Value = now.ToString("o") }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.CheckDate < now).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_DateTime_LessThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.CheckDate), Type = V2.Enums.FilterOperator.LessThanOrEqual, Value = now.ToString("o") }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.CheckDate <= now).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableDateTime_Equal()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullCheckDate), Type = V2.Enums.FilterOperator.Equal, Value = now.ToString("o") }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullCheckDate == now).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}
