using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Web
{
    public interface ICurrentUserProvider
    {
        string GetUserId();
    }

    public class CurrentUserProvider(
        IHttpContextAccessor _httpContextAccessor
    ) : ICurrentUserProvider
    {
        public string GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
