namespace BuildingBlocks.Core.Pagination
{
    public interface IPageRequest
    {
        int Page { get; init; }
        int PageSize { get; init; }
        string Sort { get; init; }
        string Filters { get; init; }
    }
}
