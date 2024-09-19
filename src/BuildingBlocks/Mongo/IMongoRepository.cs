using BuildingBlocks.Core.Model;
using System.Linq.Expressions;

namespace BuildingBlocks.Mongo
{
    public interface IReadRepository<TEntity, TId>
        where TEntity : class, IAggregate<TId>
    {
        Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAll(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> RawQuery(string rawQuery, CancellationToken cancellationToken = default, params object[] queryParams);
    }

    public interface IWriteRepository<TEntity, TId>
        where TEntity : class, IAggregate<TId>
    {
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IReadOnlyList<TEntity> entity, CancellationToken cancellationToken = default);
        Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);
    }
    public interface IMongoRepository<TEntity, TId>
        : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId>
        where TEntity : class, IAggregate<TId>
    {
    }
    public interface IMongoRepository<TEntity>
        : IReadRepository<TEntity, long>, IWriteRepository<TEntity, long>
        where TEntity : class, IAggregate<long>
    {
    }
}
