using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Enums.NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class CollectionTests
    {
        private TestDbContext dbContext;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            // »спользуем данные, предоставленные вами
            dbContext = await TestDbContext.Create(null);
        }

        [Test]
        public async Task Any_Simple_ShouldReturnEntitiesWithCollections()
        {
            // ќжидаем 2 записи, у которых есть св€занные данные
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.RelTests), Type = FilterOperator.Any }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.RelTests.Any()).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Not_Any_Simple_ShouldReturnEntitiesWithoutCollections()
        {
            // ќжидаем 4 записи, у которых нет св€занных данных
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel { Property = nameof(TestEntityModel.RelTests), Type = FilterOperator.Any, Not = true }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => !t.RelTests.Any()).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Any_WithPredicate_ShouldReturnCorrectEntity()
        {
            // ќжидаем 1 запись, у которой есть св€занный элемент с Content = "rel_1"
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel
                    {
                        Property = nameof(TestEntityModel.RelTests),
                        Type = FilterOperator.Any,
                        NestedFilter = new FilterNode
                        {
                            Filters = new List<EntityFilterBlockModel>
                            {
                                new EntityFilterBlockModel { Property = nameof(RelTestEntityModel.Content), Type = FilterOperator.Equal, Value = "rel_1" }
                            }
                        }
                    }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.RelTests.Any(r => r.Content == "rel_1")).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(expected.First().Id));
        }

        [Test]
        public async Task Count_Simple_ShouldReturnCorrectEntity()
        {
            // ќжидаем 1 запись, у которой ровно 2 св€занных элемента
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel
                    {
                        Property = nameof(TestEntityModel.RelTests),
                        Modifier = PropertyModifier.Count,
                        Type = FilterOperator.Equal,
                        Value = "2"
                    }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.RelTests.Count() == 2).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(expected.First().Id));
        }

        [Test]
        public async Task Not_Count_Simple_ShouldReturnCorrectEntities()
        {
            // ќжидаем 5 записей, у которых количество св€занных элементов не равно 1
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel
                    {
                        Property = nameof(TestEntityModel.RelTests),
                        Modifier = PropertyModifier.Count,
                        Type = FilterOperator.Equal,
                        Value = "1",
                        Not = true
                    }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.RelTests.Count() != 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }

        [Test]
        public async Task Count_WithPredicate_ShouldReturnCorrectEntities()
        {
            // ќжидаем 2 записи, у которых есть ровно 1 св€занный элемент с Type > 15
            var filter = new FilterNode
            {
                Filters = new List<EntityFilterBlockModel>
                {
                    new EntityFilterBlockModel
                    {
                        Property = nameof(TestEntityModel.RelTests),
                        Modifier = PropertyModifier.Count,
                        Type = FilterOperator.Equal,
                        Value = "1",
                        NestedFilter = new FilterNode
                        {
                            Filters = new List<EntityFilterBlockModel>
                            {
                                new EntityFilterBlockModel { Property = nameof(RelTestEntityModel.Type), Type = FilterOperator.GreaterThan, Value = "15" }
                            }
                        }
                    }
                }
            };

            var result = await dbContext.Tests.WithFilter(filter).ToListAsync();
            var expected = await dbContext.Tests.Where(t => t.RelTests.Count(r => r.Type > 15) == 1).ToListAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Count, Is.EqualTo(expected.Count));
        }
    }
}