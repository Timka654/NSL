using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class NumberTests
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
        public async Task Test_Filter_Number_Null_Equals()
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
                                Property = nameof(TestEntityModel.NullNumber),
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

            Assert.That(4, Is.EqualTo(dbContext.Tests.Where(x => x.NullNumber == null).Count()));
        }

        [Test]
        public async Task Test_Filter_Number_Null_NoEquals()
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
                                Property = nameof(TestEntityModel.NullNumber),
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
            Assert.That(output.Length, Is.EqualTo(dbContext.Tests.Where(x => x.NullNumber != null).Count()));
        }

        [Test]
        public async Task Test_Filter_Number_Equal()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.Equal, Value = "2" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number == 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_NotEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.Equal, Value = "2", Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number != 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.GreaterThan, Value = "1" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number > 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_GreaterThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.GreaterThanOrEqual, Value = "1" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number >= 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_LessThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.LessThan, Value = "1" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number < 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_LessThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.LessThanOrEqual, Value = "1" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number <= 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_Number_Complex_And()
        {
            var filter = new FilterNode
            {
                Logic = FilterLogic.And,
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.GreaterThan, Value = "0" },
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = FilterOperator.LessThan, Value = "3" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number > 0 && t.Number < 3).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_Equal()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.Equal, Value = "3" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber == 3).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_NotEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.Equal, Value = "3", Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber != 3).ToListAsync();

            // Ожидаем 5: 1 запись, где NullNumber = 2, и 4 записи, где NullNumber = null.
            // В C# (и в EF Core) `null != 3` возвращает true.
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.GreaterThan, Value = "2" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber > 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_GreaterThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.GreaterThanOrEqual, Value = "2" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber >= 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_LessThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.LessThan, Value = "3" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber < 3).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Filter_NullableNumber_LessThanOrEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = FilterOperator.LessThanOrEqual, Value = "3" }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber <= 3).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}
