namespace BuildingBlocks.Core.Model
{
    public interface IEntity<T> : IEntity where T : notnull
    {
        public T Id { get; set; }
    }

    public interface IEntity : IAuditable, IVersion
    {
    }
}
