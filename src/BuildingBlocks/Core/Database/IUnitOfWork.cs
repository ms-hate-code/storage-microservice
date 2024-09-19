namespace BuildingBlocks.Core.Database
{
    public interface IUnitOfWork : ITransactionAble
    {
    }

    public interface IUnitOfWork<TDbContext> : IUnitOfWork
        where TDbContext : class
    {
        TDbContext DbContext { get; }
    }
}
