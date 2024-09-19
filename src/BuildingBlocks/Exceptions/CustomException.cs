using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class CustomException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception(message)
    {
        public HttpStatusCode StatusCode { get; set; } = statusCode;
    }
}
