using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Core.CustomAPIResponse
{
    public record APIResponse<T>(int StatusCode, T Data) where T : class
    {
        public static APIResponse<T> Ok(T data) => new APIResponse<T>(StatusCodes.Status200OK, data);
    }
}
