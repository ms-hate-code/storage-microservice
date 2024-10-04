using BuildingBlocks.Core.Model;

namespace Metadata.FileFolder.Models;

public class FileFolder : Aggregate<Guid>
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public List<FileObject.Models.FileObject> FileObjects { get; set; }
}