using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.V2.Host
{
    public static class ModelExtensions
    {
        public static ModelBuilder HasDbFilterV2(this ModelBuilder modelBuilder, DbContext dbContext)
        {
            var typeMappingSource = dbContext.GetService<IRelationalTypeMappingSource>();
            var stringMapping = typeMappingSource.FindMapping(typeof(string));
            var boolMapping = typeMappingSource.FindMapping(typeof(bool));

            var escapeChar = '\\';
            var escapeExpr = new SqlConstantExpression(escapeChar.ToString(), stringMapping);
            var jokExpr = new SqlConstantExpression("%", stringMapping);

            SqlExpression EscapeLikePattern(SqlExpression inputExpr)
            {
                if (inputExpr is not SqlConstantExpression constantExpr || constantExpr.Value is not string input)
                    return inputExpr; // Cannot escape non-constant values at this level

                var s = input.Replace($"{escapeChar}", $"{escapeChar}{escapeChar}")
                             .Replace("%", $"{escapeChar}%")
                             .Replace("_", $"{escapeChar}_");

                return new SqlConstantExpression(s, stringMapping);
            }

            void RegisterStringFunction(string functionName, ExpressionType startJoker, ExpressionType endJoker)
            {
                var methodInfo = typeof(DbFilterFunctions).GetMethod(functionName, new[] { typeof(string), typeof(string), typeof(bool) });

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args =>
                    {
                        var source = args[0];
                        var value = EscapeLikePattern(args[1]);
                        var caseSensitive = (bool)(args[2] as SqlConstantExpression).Value;

                        SqlExpression finalSource = source;
                        SqlExpression finalValue = value;

                        if (!caseSensitive)
                        {
                            finalSource = new SqlFunctionExpression("LOWER", new[] { source }, true, new[] { true }, typeof(string), source.TypeMapping);
                            finalValue = new SqlFunctionExpression("LOWER", new[] { value }, true, new[] { true }, typeof(string), value.TypeMapping);
                        }

                        SqlExpression pattern = finalValue;

                        if (startJoker != ExpressionType.Default)
                            pattern = new SqlBinaryExpression(startJoker, jokExpr, pattern, typeof(string), stringMapping);

                        if (endJoker != ExpressionType.Default)
                            pattern = new SqlBinaryExpression(endJoker, pattern, jokExpr, typeof(string), stringMapping);

                        return new LikeExpression(finalSource, pattern, escapeExpr, boolMapping);
                    });
            }

            RegisterStringFunction(nameof(DbFilterFunctions.Contains), ExpressionType.Add, ExpressionType.Add);
            RegisterStringFunction(nameof(DbFilterFunctions.StartsWith), ExpressionType.Default, ExpressionType.Add);
            RegisterStringFunction(nameof(DbFilterFunctions.EndsWith), ExpressionType.Add, ExpressionType.Default);


            modelBuilder
                .HasDbFunction(() => DbFilterFunctions.Equals(default, default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                var caseSensitive = (bool)(args[2] as SqlConstantExpression).Value;

                    if (caseSensitive)
                    {
                        // Case-sensitive: WHERE "Column" = 'Value'
                        return new SqlBinaryExpression(
                            ExpressionType.Equal,
                            source,
                            value,
                            typeof(bool),
                            source.TypeMapping);
                    }
                    else
                    {
                        var likePattern = new SqlFunctionExpression(
                            functionName: "LOWER",
                            arguments: new[] { value },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            type: typeof(string),
                            typeMapping: value.TypeMapping);

                        var lowerSource = new SqlFunctionExpression(
                            functionName: "LOWER",
                            arguments: new[] { source },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            type: typeof(string),
                            typeMapping: source.TypeMapping);


                        return new LikeExpression(lowerSource, likePattern, escapeExpr, boolMapping);
                    }
                });

            return modelBuilder;
        }
    }
}