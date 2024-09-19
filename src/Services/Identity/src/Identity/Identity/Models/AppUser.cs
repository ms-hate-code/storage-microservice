using BuildingBlocks.Core.Event;
using BuildingBlocks.Core.Model;
using Microsoft.AspNetCore.Identity;

namespace Identity.Identity.Models
{
    public class AppUser : IdentityUser, IAggregate
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public long Version { get; set; }
        public IReadOnlyList<IDomainEvent> DomainEvents { get; }
        public IEvent[] ClearDomainEvents()
        {
            throw new NotImplementedException();
        }
    }
}
