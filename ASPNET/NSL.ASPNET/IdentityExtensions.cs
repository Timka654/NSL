using System;
using System.Security.Claims;

namespace NSL.ASPNET
{
    public static class IdentityExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static TValue? GetUserId<TValue>(this ClaimsPrincipal principal)
            where TValue : IConvertible
        {
            var v = principal.GetUserId();

            if (v != default)
                return (TValue)Convert.ChangeType(v, typeof(TValue));

            return default;
        }

        public static Guid? GetUserGuidId(this ClaimsPrincipal principal)
        {
            var v = principal.GetUserId();

            if (v != default && Guid.TryParse(v, out var guid))
                return guid;

            return default;
        }
    }
}
