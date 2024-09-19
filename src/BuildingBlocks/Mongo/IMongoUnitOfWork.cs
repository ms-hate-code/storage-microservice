using BuildingBlocks.Core.Database;

namespace BuildingBlocks.Mongo
{
    public interface IMongoUnitOfWork<TDbContext>
        : IUnitOfWork<TDbContext> where TDbContext : class, IMongoDbContext
    {
    }
}
