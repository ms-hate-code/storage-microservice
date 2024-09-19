namespace BuildingBlocks.Core.Pagination
{
    public interface IPageList<T>
        where T : class
    {
        bool HasNext { get; }
        bool HasPrevious { get; }
        int TotalPages { get; }
        int TotalCount { get; init; }
        IReadOnlyList<T> Items { get; init; }
        int Page { get; init; }
        int PageSize { get; init; }
    }
}
