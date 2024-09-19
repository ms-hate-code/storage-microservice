namespace BuildingBlocks.Constants
{
    public static class Common
    {
        public static class AuthServer
        {
            public const string STORAGE_APP = "STORAGE_APP";
            public const string STORAGE_ADMIN_APP = "STORAGE_ADMIN_APP";
            public const string STORAGE_BOTH_APP = $"{STORAGE_APP}_{STORAGE_ADMIN_APP}";
            public const string STORAGE_APP_SCOPE = "storage_app_api";
        }

        public static class SystemRole
        {
            public const string ADMIN = "ADMIN";
            public const string USER = "USER";
        }
    }
}
