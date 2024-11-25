using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace NSL.Database.EntityFramework.Filter.Host
{
    public static class ModelExtensions
    {
        public static ModelBuilder HasDbFilter(this ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => FilterExtensions.ContainsCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        value,
                        new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                        typeof(string),
                        value.TypeMapping),
                    typeof(string),
                    value.TypeMapping);

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

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: value.TypeMapping),
                        new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
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

                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: [lowerSource, likePattern],
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder.HasDbFunction(() => FilterExtensions.StartsWithCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    var likePattern =
                    new SqlBinaryExpression(
                        ExpressionType.Add,
                        value,
                        new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                        typeof(string),
                        value.TypeMapping);

                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: new[] { source, likePattern },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.StartsWithIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    var likePattern = new SqlBinaryExpression(
                        ExpressionType.Add,
                        new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { value },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: value.TypeMapping),
                        new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                        typeof(string),
                        value.TypeMapping);

                    var lowerSource = new SqlFunctionExpression(
                        functionName: "LOWER",
                        arguments: new[] { source },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        type: typeof(string),
                        typeMapping: source.TypeMapping);

                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: [lowerSource, likePattern],
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder.HasDbFunction(() => FilterExtensions.EndsWithCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
                    value,
                    typeof(string),
                    value.TypeMapping);

                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: new[] { source, likePattern },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.EndsWithIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

                    var likePattern = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlConstantExpression(Expression.Constant("%"), value.TypeMapping),
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

                    return new SqlFunctionExpression(
                        functionName: "LIKE",
                        arguments: [lowerSource, likePattern],
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        type: typeof(bool),
                        typeMapping: null);
                });

            modelBuilder
                .HasDbFunction(() => FilterExtensions.EqualsIgnoreCase(default, default))
                .HasTranslation(args =>
                {
                    var source = args.ElementAt(0);
                    var value = args.ElementAt(1);

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
