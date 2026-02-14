using System;

namespace NSL.Database.EntityFramework.Filter.V2
{
    /// <summary>
    /// Contains placeholder methods for EF Core query translation.
    /// These methods are not meant to be called directly from code, only from within LINQ expressions.
    /// </summary>
    public static class DbFilterFunctions
    {
        private const string ErrorMessage = "This method is for use with Entity Framework Core only and has no in-memory implementation.";

        public static bool Contains(string property, string value, bool caseSensitive)
            => throw new NotImplementedException(ErrorMessage);

        public static bool StartsWith(string property, string value, bool caseSensitive)
            => throw new NotImplementedException(ErrorMessage);

        public static bool EndsWith(string property, string value, bool caseSensitive)
            => throw new NotImplementedException(ErrorMessage);

        public static bool Equals(string property, string value, bool caseSensitive)
            => throw new NotImplementedException(ErrorMessage);
    }
}