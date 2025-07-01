using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class ModelExtensions
    {
        public static ModelBuilder HasDbFilter(this ModelBuilder modelBuilder, DbContext dbContext)
        {
            var typeMappingSource = dbContext.GetService<IRelationalTypeMappingSource>();
            var stringMapping = typeMappingSource.FindMapping(typeof(string));
            var boolMapping = typeMappingSource.FindMapping(typeof(bool));

            var escapeExpr = new SqlConstantExpression("\\", stringMapping);

            var jokExpr = new SqlConstantExpression("%", stringMapping);



            SqlExpression EscapeLikePattern(SqlExpression inputExpr, char escapeChar = '\\')
            {
                var input = (inputExpr as SqlConstantExpression)?.Value as string;

                var s = input.Replace($"{escapeChar}", $"{escapeChar}{escapeChar}");
                s = s.Replace("%", $"{escapeChar}%");
                s = s.Replace("_", $"{escapeChar}_");

                return new SqlConstantExpression(s, stringMapping);
            }

            modelBuilder.HasDbFunction(() => FilterExtensions.ContainsCase(default, default))
                    .HasTranslation(args =>
                    {
                        var source = args.ElementAt(0);
                        var value = EscapeLikePattern(args.ElementAt(1));

                        var likePattern = new SqlBinaryExpression(
                        ExpressionType.Add,
                        jokExpr,
                        new SqlBinaryExpression(
                            ExpressionType.Add,
                            value,
                            jokExpr,
                            typeof(string),
                            value.TypeMapping),
                        typeof(string),
                        value.TypeMapping);

                        return new LikeExpression(source, likePattern, escapeExpr, boolMapping);
                    });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.ContainsIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    jokExpr,
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: value.TypeMapping),
                        jokExpr,
                        typeof(string),
                        value.TypeMapping),
                    typeof(string),
                    value.TypeMapping);

                    var lowerSource = new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { source },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: source.TypeMapping);


                    return new LikeExpression(lowerSource, likePattern, escapeExpr, boolMapping);
                });

            modelBuilder.HasDbFunction(() => FilterExtensions.StartsWithCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                    var likePattern =
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        value,
                        jokExpr,
                        typeof(string),
                        value.TypeMapping);



                    return new LikeExpression(source, likePattern, escapeExpr, boolMapping);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.StartsWithIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                    var likePattern = new SqlBinaryExpression(
                        ExpressionType.Add,
                        new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: value.TypeMapping),
                        jokExpr,
                        typeof(string),
                        value.TypeMapping);

                    var lowerSource = new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { source },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: source.TypeMapping);

                    return new LikeExpression(lowerSource, likePattern, escapeExpr, boolMapping);
                });

            modelBuilder.HasDbFunction(() => FilterExtensions.EndsWithCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    jokExpr,
                    value,
                    typeof(string),
                    value.TypeMapping);


                    return new LikeExpression(source, likePattern, escapeExpr, boolMapping);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.EndsWithIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    jokExpr,
                        new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: value.TypeMapping),
                    typeof(string),
                    value.TypeMapping);

                    var lowerSource = new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { source },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: source.TypeMapping);


                    return new LikeExpression(lowerSource, likePattern, escapeExpr, boolMapping);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.EqualsIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = EscapeLikePattern(args.ElementAt(1));

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
                });

            return modelBuilder;
        }

    }
}
