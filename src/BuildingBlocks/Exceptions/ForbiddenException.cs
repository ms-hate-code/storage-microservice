using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class ForbiddenException(
        string message
    ) : CustomException(message, HttpStatusCode.Forbidden)
    {
    }
}
