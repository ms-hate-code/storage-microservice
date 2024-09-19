using BuildingBlocks.Core.Event;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.EFCore
{
    public interface IDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        IReadOnlyList<IDomainEvent> GetDomainEvents();
        Task<int> SaveChangeAsync(CancellationToken cancellationToken);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}
