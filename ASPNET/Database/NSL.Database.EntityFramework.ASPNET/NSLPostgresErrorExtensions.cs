using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using System;
using System.Linq;

namespace NSL.Database.EntityFramework.ASPNET
{
    public class NSLPostgresForeignKeyException(NpgsqlException exception, ForeignKeyErrorInfo foreignKeyError) : NSLPostgresException(exception)
    {
        public ForeignKeyErrorInfo ForeignKeyError { get; } = foreignKeyError;
    }

    public class NSLPostgresException(NpgsqlException exception)
    {
        public NpgsqlException Exception { get; } = exception;

        public IEntityType EntityType { get; init; }
    }

    public record ForeignKeyErrorInfo(string PropertyName, string ReferencedType, string Value);

    public static class NSLPostgresErrorExtensions
    {
        public static NSLPostgresException? TryGetNSLPostgresException(this Exception ex, DbContext db)
        {
            if (ex is DbUpdateException && ex.InnerException is PostgresException pg && pg.SqlState == "23503")
            {
                var info = ParseForeignKeyError(pg);

                var entityModel = db.Model.GetEntityTypes().FirstOrDefault(x => x.GetTableName() == info.ReferencedType);

                return new NSLPostgresForeignKeyException(pg, info) { EntityType = entityModel };
            }

            return null;
        }

        public static ForeignKeyErrorInfo ParseForeignKeyError(PostgresException pg)
        {
            var constraint = pg.ConstraintName;

            if (constraint?.StartsWith("FK_", StringComparison.OrdinalIgnoreCase) == true)
            {
                var parts = constraint.Split('_', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 4)
                {
                    var property = parts.Last();
                    var referenced = parts[^2];

                    return new ForeignKeyErrorInfo(property, referenced, ExtractKeyValue(pg.Detail));
                }
            }

            // fallback
            return new ForeignKeyErrorInfo("UnknownProperty", "UnknownType", ExtractKeyValue(pg.Detail));
        }

        public static string? ExtractKeyValue(string? detail)
        {
            // DETAIL: Key (SupportTicketId)=(aa7ce370...) is not present...
            if (string.IsNullOrWhiteSpace(detail))
                return null;

            var start = detail.IndexOf("=(");
            var end = detail.IndexOf(')', start + 2);

            if (start >= 0 && end > start)
                return detail.Substring(start + 2, end - start - 2);

            return null;
        }
    }
}
