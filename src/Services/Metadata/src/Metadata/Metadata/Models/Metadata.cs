using BuildingBlocks.Core.Model;

namespace Metadata.Metadata.Models;

public class Metadata : Aggregate<Guid>
{
    public Guid UserId { get; set; }
    public Guid? FileId { get; set; }
    public string DisplayFileName { get; set; }
    public string UploadFileName { get; set; }
    public string FileUrl { get; set; }
}