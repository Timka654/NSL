using Microsoft.EntityFrameworkCore;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;

namespace NSL.Database.EntityFramework.Filter.V2.Tests
{
    [TestFixture]
    public class SelectorTests
    {
        private TestDbContext dbContext;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            dbContext = await TestDbContext.Create(DateTime.UtcNow);
        }

        [Test]
        public async Task Test_WithIncludes()
        {
            // Акт
            var query = dbContext.Tests.WithIncludes(new[] { nameof(TestEntityModel.RelTests) });
            var result = await query.FirstOrDefaultAsync(t => t.RelTests.Any());

            // Анализ
            // Проверяем, что связанная сущность действительно была загружена
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RelTests, Is.Not.Null);
            Assert.That(result.RelTests.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task Test_SelectDynamic_Simple()
        {
            // Аранжировка
            var properties = new[] { nameof(TestEntityModel.Id), nameof(TestEntityModel.Content) };

            // Акт
            var result = await dbContext.Tests.SelectDynamic(properties).FirstOrDefaultAsync();

            // Анализ
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.ContainsKey(nameof(TestEntityModel.Id)), Is.True);
            Assert.That(result.ContainsKey(nameof(TestEntityModel.Content)), Is.True);
        }

        [Test]
        public async Task Test_SelectDynamic_Nested()
        {
            // Аранжировка
            var properties = new[] { nameof(TestEntityModel.Id), $"{nameof(TestEntityModel.RelTests)}.{nameof(RelTestEntityModel.Content)}" };

            // Акт
            var result = await dbContext.Tests
                .WithIncludes(new[] { nameof(TestEntityModel.RelTests) }) // Важно сначала подгрузить данные
                .Where(t => t.RelTests.Any())
                .SelectDynamic(properties)
                .FirstOrDefaultAsync();

            // Анализ
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContainsKey(nameof(TestEntityModel.Id)), Is.True);
            Assert.That(result.ContainsKey(nameof(TestEntityModel.RelTests)), Is.True);

            var nestedRelTests = result[nameof(TestEntityModel.RelTests)] as IEnumerable<Dictionary<string, object>>;
            Assert.That(nestedRelTests, Is.Not.Null);
            Assert.That(nestedRelTests.Any(), Is.True);

            var firstRelTest = nestedRelTests.First();
            Assert.That(firstRelTest.ContainsKey(nameof(RelTestEntityModel.Content)), Is.True);
            Assert.That(firstRelTest.Count, Is.EqualTo(1)); // Убеждаемся, что выбрано только поле Content
        }
    }
}