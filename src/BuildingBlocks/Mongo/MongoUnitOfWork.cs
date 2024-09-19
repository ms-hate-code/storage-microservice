
namespace BuildingBlocks.Mongo
{
    public class MongoUnitOfWork<TDbContext>
    (
        TDbContext context
    ) : IMongoUnitOfWork<TDbContext> where TDbContext : class, IMongoDbContext
    {
        public TDbContext DbContext => context;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await DbContext.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await DbContext.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await DbContext.RollbackTransactionAsync(cancellationToken);
        }
    }
}
