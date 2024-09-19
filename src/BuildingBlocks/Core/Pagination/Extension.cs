using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace BuildingBlocks.Core.Pagination
{
    public static class Extension
    {
        public static async Task<IPageList<T>> ProcessingDataAsync<T>(
            this IQueryable<T> source,
            IPageRequest request,
            ISieveProcessor sieveProcessor,
            CancellationToken cancellationToken = default
        )
        where T : class
        {
            SieveModel sieveModel = new()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sorts = request.Sort,
                Filters = request.Filters
            };

            int totalCount = await source.CountAsync(cancellationToken: cancellationToken);

            List<T> items = await sieveProcessor
                .Apply(sieveModel, source, applyFiltering: true, applyPagination: true)
                .ToListAsync(cancellationToken);

            return new PageList<T>(items, request.Page, request.PageSize, totalCount);
        }
    }
}
