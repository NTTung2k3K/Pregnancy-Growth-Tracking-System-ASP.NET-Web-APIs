using BabyCare.Core.Base;

namespace BabyCare.ModelViews.ChildModelView
{
    public class SearchChildByUserId : SearchOptimizeRequest
    {
        public Guid userId { get; set; }
    }
    public class SearchOptimizeRequest : BaseSearchRequest
    {

        // Bộ lọc (filter)
        public DateTime? FromDate { get; set; } // Ngày bắt đầu lọc
        public DateTime? ToDate { get; set; } // Ngày kết thúc lọc

        // Sắp xếp (sort)
        public string? SortBy { get; set; } // Cột cần sắp xếp
        public bool IsDescending { get; set; } = false; // true nếu sắp xếp giảm dần
    }
}
