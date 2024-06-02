namespace System.Security
{
    namespace Claims
    {
        public static class Extensions
        {
            public static T GetUserId<T>(this ClaimsPrincipal principal) where T : struct
            {
                var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (value != null)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return default;
            }

            public static string GetUserId(this ClaimsPrincipal principal) => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            public static string GetUserFullName(this ClaimsPrincipal principal)
            {
                var displayName = string.Join(' ', principal.FindFirst(ClaimTypes.GivenName)?.Value, principal.FindFirst(ClaimTypes.Surname)?.Value);
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = principal.FindFirst(ClaimTypes.Name)?.Value;
                }

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = "N/A";
                }

                return displayName;
            }
        }
    }
}
