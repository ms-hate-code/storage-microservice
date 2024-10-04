using BuildingBlocks.Exceptions;

namespace Metadata.FileObject.Exceptions;

public class FileObjectNotFoundException(string message)
    : NotFoundException(message)
{
}