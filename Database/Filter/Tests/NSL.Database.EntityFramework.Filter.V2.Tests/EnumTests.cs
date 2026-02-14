using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class EnumTests
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
        public async Task Test_Filter_Enum_Null_Equals()
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
                                Property = nameof(TestEntityModel.NullEnum),
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
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullEnum == null)));
        }

        [Test]
        public async Task Test_Filter_Enum_Null_NoEquals()
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
                                Property = nameof(TestEntityModel.NullEnum),
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
            Assert.That(output.Length, Is.EqualTo(await dbContext.Tests.CountAsync(x => x.NullEnum != null)));
        }

        [Test]
        public async Task Test_Filter_Enum_Equal_ByName()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.Equal, Value = "Second" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Enum == TestEnum.Second).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Enum_Equal_ByName_CaseInsensitive()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.Equal, Value = "third" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Enum == TestEnum.Third).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Enum_Equal_ByValue()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.Equal, Value = "1" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Enum == TestEnum.First).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Enum_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.GreaterThan, Value = "First" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Enum > TestEnum.First).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableEnum_Equal_ByName()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullEnum), Type = FilterOperator.Equal, Value = "Second" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullEnum == TestEnum.Second).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}
