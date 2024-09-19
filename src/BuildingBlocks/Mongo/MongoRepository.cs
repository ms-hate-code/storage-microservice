using BuildingBlocks.Core.Model;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BuildingBlocks.Mongo
{
    public class MongoRepository<TEntity, TId>
    (
        IMongoDbContext _context
    )
    : IMongoRepository<TEntity, TId> where TEntity : class, IAggregate<TId>
    {
        protected IMongoCollection<TEntity> DbSet => _context.GetCollections<TEntity>();

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet
                .InsertOneAsync(entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet
                .DeleteOneAsync(x => x.Id.Equals(entity.Id), cancellationToken);
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await DbSet
                .DeleteOneAsync(predicate, cancellationToken);
        }

        public async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            await DbSet
                .DeleteOneAsync(x => x.Id.Equals(id), cancellationToken);
        }

        public async Task DeleteRangeAsync(IReadOnlyList<TEntity> entity, CancellationToken cancellationToken = default)
        {
            await DbSet.DeleteManyAsync(x => entity
                .Select(x => x.Id)
                .Contains(x.Id), cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<TEntity> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await FindOneAsync(x => x.Id.Equals(id), cancellationToken);
        }

        public async Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Find(filter).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> GetAll(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AsQueryable()
                .ToListAsync(cancellationToken);
        }

        public Task<IReadOnlyList<TEntity>> RawQuery(string rawQuery, CancellationToken cancellationToken = default, params object[] queryParams)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet
                .ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity, new ReplaceOptions(), cancellationToken);
        }
    }
}
