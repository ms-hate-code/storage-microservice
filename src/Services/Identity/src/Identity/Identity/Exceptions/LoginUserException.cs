using BuildingBlocks.Exceptions;

namespace Identity.Identity.Exceptions
{
    internal class LoginUserException
        (string message) : AppException(message)
    {
    }
}
