using Microsoft.EntityFrameworkCore;

namespace SchoolManagement.Common
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;

            var totalRecords = await query.CountAsync();

            // Tính toán Skip và Take
            var items = await query.Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageIndex = pageIndex,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
        }
    }
}
