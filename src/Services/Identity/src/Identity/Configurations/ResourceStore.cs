using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Identity.Configurations
{
    public class ResourceStore : IResourceStore
    {
        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            return Task.FromResult(
                Config.ApiResources
                    .Where(apiResource => apiResourceNames.Contains(apiResource.Name))
            );
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(
                Config.ApiResources
                    .Where(apiResource => apiResource.Scopes.Any(scope => scopeNames.Contains(scope)))
            );
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(
                Config.ApiScopes
                    .Where(apiScope => scopeNames.Contains(apiScope.Name))
            );
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(
                Config.IdentityResources
                    .Where(identityResource => scopeNames.Contains(identityResource.Name))
            );
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            return Task.FromResult(
                new Resources(
                    Config.IdentityResources,
                    Config.ApiResources,
                    Config.ApiScopes
                )
            );
        }
    }
}
