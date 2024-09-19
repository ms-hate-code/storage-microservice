using BuildingBlocks.PersistMessageStore.Model;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.PersistMessageStore.Interfaces
{
    public interface IPersistMessageDbContext
    {
        DbSet<PersistMessage> PersistMessages { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteTransactionalAsync(CancellationToken cancellationToken = default);
    }
}
