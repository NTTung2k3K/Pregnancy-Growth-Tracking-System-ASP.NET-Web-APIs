using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Core.Base
{
    public class BaseSearchRequest
    {
        public string? SearchValue { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }

    }
}
