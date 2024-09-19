using BuildingBlocks.Exceptions;

namespace Identity.Identity.Exceptions
{
    public class InvalidCredentialsException
        (string message) : ValidationException(message)
    {
    }
}
