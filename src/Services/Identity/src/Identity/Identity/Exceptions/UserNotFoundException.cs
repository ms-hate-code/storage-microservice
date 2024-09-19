using BuildingBlocks.Exceptions;

namespace Identity.Identity.Exceptions
{
    public class UserNotFoundException(string message)
        : NotFoundException(message)
    {
    }
}
