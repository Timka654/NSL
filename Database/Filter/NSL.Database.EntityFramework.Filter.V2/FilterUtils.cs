using NSL.Database.EntityFramework.Filter.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NSL.Database.EntityFramework.Filter.V2
{
    public class FilterUtils
    {
        /// <summary>
        /// Joins property names into a single path string.
        /// </summary>
        /// <param name="properties">An array of property names.</param>
        /// <returns>A dot-separated path string.</returns>
        public static string GetPath(params string[] properties)
        {
            if (properties == null || properties.Length == 0)
                throw new ArgumentException("Properties cannot be null or empty.", nameof(properties));

            return string.Join(".", properties);
        }

        /// <summary>
        /// Builds a type-safe property path string from a lambda expression.
        /// </summary>
        /// <example>
        /// GetPath&lt;MyClass&gt;(x => x.User.Address.City) returns "User.Address.City".
        /// </example>
        /// <typeparam name="T">The root type.</typeparam>
        /// <param name="propertyExpression">The expression representing the property path.</param>
        /// <returns>A dot-separated path string.</returns>
        public static string GetPath<T>(Expression<Func<T, object>> propertyExpression)
        {
            var parts = new List<string>();
            var currentExpression = propertyExpression.Body;

            // Handle UnaryExpression (e.g., Convert from value type to object)
            if (currentExpression is UnaryExpression unary)
            {
                currentExpression = unary.Operand;
            }

            while (currentExpression is MemberExpression member)
            {
                parts.Add(member.Member.Name);
                currentExpression = member.Expression;
            }

            if (!parts.Any() || !(currentExpression is ParameterExpression))
            {
                throw new ArgumentException("The provided expression is not a valid property path.", nameof(propertyExpression));
            }

            parts.Reverse();
            return string.Join(".", parts);
        }
    }
}
