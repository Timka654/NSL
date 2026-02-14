using BenchmarkDotNet.Attributes;
using NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Enums.NSL.Database.EntityFramework.Filter.V2.Enums;
using NSL.Database.EntityFramework.Filter.V2.Host;
using NSL.Database.EntityFramework.Filter.V2.Models;
using NSL.Database.EntityFramework.Filter.V2.Tests.Data;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.V2.Benchmarks
{
    [MemoryDiagnoser] // Добавляем диагностику памяти
    [MarkdownExporter, RPlotExporter] // Экспортируем результаты в красивые форматы
    public class OperatorBenchmark
    {
        private EntityFilterBuilder<TestEntityModel> builder;
        private ParameterExpression parameter;

        // --- Объекты фильтров для каждого теста ---
        private EntityFilterBlockModel equalInt;
        private EntityFilterBlockModel equalStringSensitive;
        private EntityFilterBlockModel equalStringInsensitive;
        private EntityFilterBlockModel equalEnumName;
        private EntityFilterBlockModel equalEnumValue;
        private EntityFilterBlockModel greaterThanEnum;
        private EntityFilterBlockModel containsStringInsensitive;
        private EntityFilterBlockModel anySimple;
        private EntityFilterBlockModel anyPredicate;
        private EntityFilterBlockModel countSimple;
        private EntityFilterBlockModel notEqualInt;

        [GlobalSetup]
        public void Setup()
        {
            builder = new EntityFilterBuilder<TestEntityModel>();
            parameter = Expression.Parameter(typeof(TestEntityModel), "p");

            // Инициализируем все модели фильтров один раз, чтобы это не влияло на замеры
            equalInt = new() { Property = nameof(TestEntityModel.Number), Type = FilterOperator.Equal, Value = "123" };
            equalStringSensitive = new() { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Equal, Value = "abc", CaseSensitive = true };
            equalStringInsensitive = new() { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Equal, Value = "abc", CaseSensitive = false };
            equalEnumName = new() { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.Equal, Value = "First" };
            equalEnumValue = new() { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.Equal, Value = "1" };
            greaterThanEnum = new() { Property = nameof(TestEntityModel.Enum), Type = FilterOperator.GreaterThan, Value = "1" };
            containsStringInsensitive = new() { Property = nameof(TestEntityModel.Content), Type = FilterOperator.Contains, Value = "abc", CaseSensitive = false };
            anySimple = new() { Property = nameof(TestEntityModel.RelTests), Type = FilterOperator.Any };
            anyPredicate = new() { Property = nameof(TestEntityModel.RelTests), Type = FilterOperator.Any, NestedFilter = new FilterNode { Filters = [new() { Property = "Content", Type = FilterOperator.Equal, Value = "a" }] } };
            countSimple = new() { Property = nameof(TestEntityModel.RelTests), Modifier = PropertyModifier.Count, Type = FilterOperator.Equal, Value = "5" };
            notEqualInt = new() { Property = nameof(TestEntityModel.Number), Type = FilterOperator.Equal, Value = "123", Not = true };
        }

        [Benchmark(Description = "Equal on Int")]
        public void Benchmark_Equal_OnInt() => builder.BuildExpression(parameter, equalInt);

        [Benchmark(Description = "Equal on String (Sensitive)")]
        public void Benchmark_Equal_OnString_CaseSensitive() => builder.BuildExpression(parameter, equalStringSensitive);

        [Benchmark(Description = "Equal on String (Insensitive)")]
        public void Benchmark_Equal_OnString_CaseInsensitive() => builder.BuildExpression(parameter, equalStringInsensitive);

        [Benchmark(Description = "Equal on Enum (by Name)")]
        public void Benchmark_Equal_OnEnum_ByName() => builder.BuildExpression(parameter, equalEnumName);

        [Benchmark(Description = "Equal on Enum (by Value)")]
        public void Benchmark_Equal_OnEnum_ByValue() => builder.BuildExpression(parameter, equalEnumValue);

        [Benchmark(Description = "GreaterThan on Enum")]
        public void Benchmark_GreaterThan_OnEnum() => builder.BuildExpression(parameter, greaterThanEnum);

        [Benchmark(Description = "Contains (Insensitive)")]
        public void Benchmark_Contains_CaseInsensitive() => builder.BuildExpression(parameter, containsStringInsensitive);

        [Benchmark(Description = "Any (Simple)")]
        public void Benchmark_Any_Simple() => builder.BuildExpression(parameter, anySimple);

        [Benchmark(Description = "Any (with Predicate)")]
        public void Benchmark_Any_WithPredicate() => builder.BuildExpression(parameter, anyPredicate);

        [Benchmark(Description = "Count (Simple)")]
        public void Benchmark_Count_Simple() => builder.BuildExpression(parameter, countSimple);

        [Benchmark(Description = "Not on Equal Int")]
        public void Benchmark_Not_On_Equal_Int() => builder.BuildExpression(parameter, notEqualInt);
    }
}