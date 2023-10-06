using System.Security.Claims;
namespace System.Security
{
    public static class Extensions
    {
        public static void ReplaceClaim(this ClaimsIdentity identity, string type, string value)
        {
            var claim = identity.FindFirst(type);
            if (claim != null) identity.RemoveClaim(claim);
            identity.AddClaim(new Claim(type, value));
        }
    }

}
