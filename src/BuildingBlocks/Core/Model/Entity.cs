
namespace BuildingBlocks.Core.Model
{
    public class Entity<T> : IEntity<T>
    {
        public T Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public long Version { get; set; }
    }
}
