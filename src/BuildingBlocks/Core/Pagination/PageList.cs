namespace BuildingBlocks.Core.Pagination
{
    public record PageList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount) : IPageList<T>
        where T : class
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public static PageList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            return new(items, page, pageSize, totalCount);
        }
    }
}
