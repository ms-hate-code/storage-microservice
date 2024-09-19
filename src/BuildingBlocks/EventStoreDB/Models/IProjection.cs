namespace BuildingBlocks.EventStoreDB.Models
{
    public interface IProjection
    {
        void When(object @event);
    }
}
