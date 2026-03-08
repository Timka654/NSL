using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class FloatNumberTests
    {
        TestDbContext dbContext;

        DateTime now;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            now = DateTime.UtcNow;
            dbContext = await TestDbContext.Create(now);
        }

        [Test]
        public async Task Test_Filter_FloatNumber_Null_Equals()
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
                                Property = nameof(TestEntityModel.NullFloatNumber),
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
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullFloatNumber == null)));
        }

        [Test]
        public async Task Test_Filter_FloatNumber_Null_NoEquals()
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
                                Property = nameof(TestEntityModel.NullFloatNumber),
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
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullFloatNumber != null)));
        }

        // --- Tests for FloatNumber (double) ---

        [Test]
        public async Task Test_Filter_FloatNumber_Equal()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.FloatNumber), Type = FilterOperator.Equal, Value = "2.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.FloatNumber == 2.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_FloatNumber_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.FloatNumber), Type = FilterOperator.GreaterThan, Value = "1.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.FloatNumber > 1.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_FloatNumber_LessThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.FloatNumber), Type = FilterOperator.LessThan, Value = "1.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.FloatNumber < 1.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        // --- Tests for NullFloatNumber (double?) ---

        [Test]
        public async Task Test_Filter_NullableFloatNumber_Equal()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullFloatNumber), Type = FilterOperator.Equal, Value = "1.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullFloatNumber == 1.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableFloatNumber_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullFloatNumber), Type = FilterOperator.GreaterThan, Value = "1.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullFloatNumber > 1.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableFloatNumber_LessThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullFloatNumber), Type = FilterOperator.LessThanOrEqual, Value = "1.0" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullFloatNumber <= 1.0).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}
