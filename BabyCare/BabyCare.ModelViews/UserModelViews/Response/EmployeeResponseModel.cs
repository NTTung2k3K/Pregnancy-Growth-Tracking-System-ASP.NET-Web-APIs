using BabyCare.ModelViews.RoleModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.UserModelViews.Response
{
    public class EmployeeResponseModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Image { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public RoleModelView Role { get; set; }
    }
}
