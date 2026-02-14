using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class BooleanTests
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
        public async Task Test_Filter_Boolean_Null_Equals()
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
                                Property = nameof(TestEntityModel.NullableBooleanValue),
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

            Assert.That(4, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullableBooleanValue == null)));
        }

        [Test]
        public async Task Test_Filter_Boolean_Null_NoEquals()
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
                                Property = nameof(TestEntityModel.NullableBooleanValue),
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

            Assert.That(2, Is.EqualTo(output.Length));
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullableBooleanValue != null)));
        }

        [Test]
        public async Task Test_Filter_Boolean_IsTrue()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.BooleanValue), Type = FilterOperator.Equal, Value = "true" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.BooleanValue == true).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Boolean_IsFalse()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.BooleanValue), Type = FilterOperator.Equal, Value = "false" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.BooleanValue == false).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableBoolean_IsTrue()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullableBooleanValue), Type = FilterOperator.Equal, Value = "true" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullableBooleanValue == true).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableBoolean_IsFalse()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullableBooleanValue), Type = FilterOperator.Equal, Value = "false" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullableBooleanValue == false).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}
