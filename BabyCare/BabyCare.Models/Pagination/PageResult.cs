using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Models.Pagination
{
    public class PaginatedResult<T> : PaginatedResultBase
    {
        public List<T> Items { get; set; }
        // Constructor to initialize items and pagination details
        public PaginatedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalRecords = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        // Static method to create an asynchronous paginated result
        public static async Task<PaginatedResult<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}
