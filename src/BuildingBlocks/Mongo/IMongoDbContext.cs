using BuildingBlocks.Core.Database;
using MongoDB.Driver;

namespace BuildingBlocks.Mongo
{
    public interface IMongoDbContext : ITransactionAble
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IMongoCollection<T> GetCollections<T>(string? name = null);
        void AddCommand(Func<Task> func);
    }
}
