using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class SearchAppointmentByUserId : SearchOptimizeRequest
    {
        public Guid userId { get; set; }
    }
    public class SearchOptimizeRequest : BaseSearchRequest
    {

        // Bộ lọc (filter)
        public DateTime? FromDate { get; set; } // Ngày bắt đầu lọc
        public DateTime? ToDate { get; set; } // Ngày kết thúc lọc
        public int? Status { get; set; } // Lọc theo trạng thái

        // Sắp xếp (sort)
        public string? SortBy { get; set; } // Cột cần sắp xếp
        public bool IsDescending { get; set; } = false; // true nếu sắp xếp giảm dần
    }
}
