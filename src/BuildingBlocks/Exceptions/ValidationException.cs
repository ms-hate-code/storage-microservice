using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class ValidationException(
        string message
    ) : CustomException(message, HttpStatusCode.BadRequest)
    {
    }
}
