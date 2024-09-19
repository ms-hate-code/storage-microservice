using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class NotFoundException(
        string message
    ) : CustomException(message, HttpStatusCode.NotFound)
    {
    }
}
