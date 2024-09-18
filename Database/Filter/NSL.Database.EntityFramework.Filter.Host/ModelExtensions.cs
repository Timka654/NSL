using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class ModelExtensions
    {
        public static ModelBuilder HasPostgresFilter(this ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => FilterExtensions.ContainsCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    // Получаем значение из SqlConstantExpression и убеждаемся, что это строка
                    string patternValue = value is SqlConstantExpression constValue
                        ? constValue.Value?.ToString()
                        : value.ToString();

                    if (patternValue == null)
                    {
                        throw new InvalidOperationException("Value cannot be null.");
                    }

                    // Используем SQL-представление для LIKE
                    var likePattern = new SqlConstantExpression(
                        Expression.Constant($"%{patternValue}%"),
                        source.TypeMapping);

                    // Возвращаем результат вызова LIKE напрямую, так как он возвращает boolean
                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: new[] { source, likePattern },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.ContainsIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    // Получаем значение из SqlConstantExpression и убеждаемся, что это строка
                    string patternValue = value is SqlConstantExpression constValue
                        ? constValue.Value?.ToString().ToLower()
                        : value.ToString().ToLower();

                    if (patternValue == null)
                    {
                        throw new InvalidOperationException("Value cannot be null.");
                    }

                    // Создаем вызовы LOWER() для аргументов
                    var lowerSource = new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { source },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: source.TypeMapping);

                    var likePattern = new SqlConstantExpression(
                        Expression.Constant($"%{patternValue}%"),
                        source.TypeMapping);

                    // Возвращаем результат вызова LIKE напрямую, так как он возвращает boolean
                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: [lowerSource, likePattern],
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            return modelBuilder;
        }

    }
}
