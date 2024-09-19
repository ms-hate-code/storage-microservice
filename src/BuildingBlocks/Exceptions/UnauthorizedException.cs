using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class UnauthorizedException(
        string message
    ) : CustomException(message, HttpStatusCode.Unauthorized)
    {
    }
}
