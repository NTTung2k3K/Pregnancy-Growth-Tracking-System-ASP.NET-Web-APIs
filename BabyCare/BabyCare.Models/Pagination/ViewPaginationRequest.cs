using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Models.Pagination
{
  public class ViewPaginationRequest : PagingRequestBase
  {
    public string? Keyword { get; set; }
    public string? SortType { get; set; }
    public Guid Id { get; set; }

  }
}
