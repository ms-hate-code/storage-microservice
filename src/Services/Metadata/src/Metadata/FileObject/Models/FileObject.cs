using BuildingBlocks.Core.Model;

namespace Metadata.FileObject.Models;

public class FileObject : Aggregate<Guid>
{
    public Guid FileFolderId { get; set; }
    public string UserId { get; set; }
    public string FileName { get; set; }
    public string DisplayFileName { get; set; }
    public string Url { get; set; }
    public string ContentType { get; set; }
    public double Size { get; set; }
    public FileFolder.Models.FileFolder FileFolder { get; set; }
}