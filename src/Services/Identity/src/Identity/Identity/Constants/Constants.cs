namespace Identity.Identity.Constants
{
    public static class Constants
    {
        public static class StandardScopes
        {
            public const string IdentityApi = "identity-api";
        }

        public static class RedisKeyPrefix
        {
            public const string OperationalStore = "Identity:Operational";
            public const string ConfigureStore = "Identity:ConfigureStore";
        }
    }
}
