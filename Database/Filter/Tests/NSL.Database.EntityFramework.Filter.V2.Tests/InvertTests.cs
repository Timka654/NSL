using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{

    [TestFixture]
    public class InvertTests
    {
        TestDbContext dbContext;

        DateTime now;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            now = DateTime.UtcNow;
            dbContext = await TestDbContext.Create(now);
        }

        #region String Tests

        [Test]
        public async Task Test_String_NotEqual_CaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = Enums.FilterOperator.Equal,
                                CaseSensitive = true,
                                Value = "aabb",
                                Not = true
                            }
                        }
            };
            var rquery = dbContext.Tests.WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            var expected = await dbContext.Tests.Where(x => x.Content != "aabb").ToListAsync();

            Assert.That(output.Length, Is.EqualTo(5));
            Assert.That(output.Length, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_String_NotEqual_NoCaseSensitive()
        {
            FilterNode fn = new FilterNode()
            {
                Filters = new List<EntityFilterBlockModel>()
                        {
                            new EntityFilterBlockModel()
                            {
                                Property = nameof(TestEntityModel.Content),
                                Type = Enums.FilterOperator.Equal,
                                CaseSensitive = false,
                                Value = "aabb",
                                Not = true
                            }
                        }
            };
            var rquery = dbContext.Tests.WithFilter(fn);

            var output = await rquery.ToArrayAsync();
            var expected = await dbContext.Tests.Where(x => !EF.Functions.ILike(x.Content, "aabb")).ToListAsync();

            Assert.That(output.Length, Is.EqualTo(1));
            Assert.That(output.Length, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_String_Not_Contains_CaseSensitive()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Content), Type = Enums.FilterOperator.Contains, Value = "bb", CaseSensitive = true, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            // Исходя из того, что LIKE для вашей БД чувствителен к регистру, но имеет особую коллацию
            var expected = await dbContext.Tests.Where(x => !EF.Functions.Like(x.Content, "%bb%")).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        #endregion

        #region Number Tests

        [Test]
        public async Task Test_Number_NotEqual()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = Enums.FilterOperator.Equal, Value = "2", Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.Number != 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_NullableNumber_NotEqual_ToValue()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.NullNumber), Type = Enums.FilterOperator.Equal, Value = "3", Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.NullNumber != 3).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Number_Not_GreaterThan()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.Number), Type = Enums.FilterOperator.GreaterThan, Value = "1", Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !(t.Number > 1)).ToListAsync(); // Эквивалентно t.Number <= 1

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        #endregion

        #region Collection Tests

        [Test]
        public async Task Test_Not_Any_Simple()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.RelTests), Type = Enums.FilterOperator.Any, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !t.RelTests.Any()).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Test_Not_Any_WithPredicate()
        {
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel
                    {
                        Property = nameof(TestEntityModel.RelTests),
                        Type = Enums.FilterOperator.Any,
                        Not = true,
                        NestedFilter = new FilterNode
                        {
                            Filters = new List<EntityFilterBlockModel>
                            {
                                new EntityFilterBlockModel { Property = nameof(RelTestEntityModel.Content), Type = Enums.FilterOperator.Equal, Value = "rel_1" }
                            }
                        }
                    }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !t.RelTests.Any(r => r.Content == "rel_1")).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        #endregion
    }
}
