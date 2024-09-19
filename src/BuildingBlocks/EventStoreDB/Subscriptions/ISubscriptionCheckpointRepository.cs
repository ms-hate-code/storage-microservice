
namespace BuildingBlocks.EventStoreDB.Subscriptions
{
    public interface ISubscriptionCheckpointRepository
    {
        Task<ulong?> Load(string subscriptionId, CancellationToken cancellationToken);
        Task Store(string subscriptionId, ulong position, CancellationToken cancellationToken);
    }
}
