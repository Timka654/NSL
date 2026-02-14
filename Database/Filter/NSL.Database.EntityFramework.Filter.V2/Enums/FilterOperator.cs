namespace NSL.Database.EntityFramework.Filter.V2.Enums
{
    public enum FilterOperator
    {
        // General
        Equal,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,

        // String operations
        Contains,
        StartsWith,
        EndsWith,

        // Collection operations
        /// <summary>
        /// Checks if any element in a collection satisfies a nested filter condition.
        /// Uses the NestedFilter property.
        /// </summary>
        Any,
    }
}