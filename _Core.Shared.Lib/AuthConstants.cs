namespace _Core.Shared.Lib
{
    public static class AuthConstants
    {
        public const string CookieScheme = "AlmesSecureCookie";

        public const string TokenHeaderName = "Authorization";
        public const string TokenPrefix = "Bearer ";

        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";
        public const string RoleManager = "Manager";
    }
}