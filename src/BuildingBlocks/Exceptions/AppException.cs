using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class AppException(
       string message
    ) : CustomException(message, HttpStatusCode.InternalServerError)
    {
    }
}
