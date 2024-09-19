using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Options;

namespace Identity.Configurations
{
    public class ClientStore(
        IOptions<AuthOptions> authOptions
    ) : IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var authValue = authOptions.Value;

            if (clientId != authValue.ClientId)
            {
                return Task.FromResult<Client>(new());
            }

            return Task.FromResult(new Client
            {
                ClientId = authValue.ClientId,
                ClientSecrets = {
                    new Secret(authValue.ClientSecret.Sha256())
                },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = {
                    authValue.Scope
                },
                UpdateAccessTokenClaimsOnRefresh = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AllowOfflineAccess = true,
                AccessTokenLifetime = 3600,  // authorize the client to access protected resources
                IdentityTokenLifetime = 3600 // authenticate the user
            });
        }
    }
}
