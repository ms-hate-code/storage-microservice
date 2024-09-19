using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class BadRequestException(
        string message
    ) : CustomException(message, HttpStatusCode.BadRequest)
    {
    }
}
