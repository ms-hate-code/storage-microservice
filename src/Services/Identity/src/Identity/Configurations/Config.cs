using BuildingBlocks.Constants;
using Identity.Identity.Constants;
using IdentityServer4.Models;
using Microsoft.Extensions.Options;

namespace Identity.Configurations
{
    public class Config
    {
        private readonly IOptions<AuthOptions> authOptions;
        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
        ];

        public static IEnumerable<ApiScope> ApiScopes =>
        [
            new(Common.AuthServer.STORAGE_APP_SCOPE)
        ];

        public static IEnumerable<ApiResource> ApiResources =>
        [
            new(Constants.StandardScopes.IdentityApi)
        ];
    }
}
