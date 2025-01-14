using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Models.Pagination
{
  public class PagingRequestBase
  {
    public int? pageIndex { get; set; }
        public int? pageSize{ get; set; }

    }
}
