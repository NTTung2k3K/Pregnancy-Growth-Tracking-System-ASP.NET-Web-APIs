using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.PaymentModelView.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.UserMembershipModelView.Response
{
    public class UserMembershipResponse
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public int GrowthChartShareCount { get; set; }
        public int AddedRecordCount { get; set; }

        public UserResponseModel User { get; set; }
        public MPResponseModel Package { get; set; }

    }
}
