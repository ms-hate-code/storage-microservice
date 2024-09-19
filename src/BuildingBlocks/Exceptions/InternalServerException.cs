using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class InternalServerException(
        string message
    ) : CustomException(message, HttpStatusCode.InternalServerError)
    {
    }
}
