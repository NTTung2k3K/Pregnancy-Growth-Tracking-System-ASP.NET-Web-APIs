using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Core.Utils
{
    public class SystemConstant
    {
        public static int PAGE_SIZE = 10;
        public static int MAX_SLOT_AVAILABLE_APPOINTMENT = 4;
        public static int DURATION_UPDATE_APPOINTMENT = 4;
        public static int MAX_PER_COMMENT = 10;


        public class MembershipPackageStatus
        {
            public static string Active = "Active";
            public static string InActive = "InActive";

        }
        public enum BlogStatus
        {
            InActive = 0,
            Active = 1,
        }
        public enum FeedbackStatus
        {
            BANNED = 0,
            Active = 1,
        }
        public enum GrowthChartStatus
        {
            Unshared = 0,
            Shared = 1,
            Blocked = 2,
            Answered = 3,
        }
        public class Provider
        {
            public static string GOOGLE = "Google";
        }
        public class Role
        {
            public static string ADMIN = "Admin";
            public static string DOCTOR = "Doctor";
            public static string USER = "User";
        }
        public enum Gender
        {
            Female = 0,
            Male = 1,
        }

        public enum AppointmentStatus
        {
            Pending = 1,
            Confirmed = 2, // Đã xác nhận
            InProgress = 3, // Đang diễn ra
            Completed = 4, // Đã hoàn thành
            CancelledByUser = 5, // Hủy bởi người dùng
            CancelledByDoctor = 6, // Hủy bởi bác sĩ
            NoShow = 7, // Người dùng không đến
            Rescheduled = 8, // Đã dời lịch
            Failed = 9 // Thất bại (do lỗi hệ thống hoặc các lý do khác)

        }
        public enum AppointmentTemplatesStatus
        {
            Active = 1,
            InActive = 0,

        }

        public enum UserStatus
        {
            Active = 1,
            InActive = 0,

        }
        public enum EmployeeStatus
        {
            Active = 1,
            InActive = 0
        }
        public enum PackageStatus
        {
            Active = 1,
            InActive = 0,
        }
        public enum PackageLevel
        {
            None = 0,
            Bronze = 1,
            Silver = 2,
            Gold = 3,
            Premium = 4,
        }
    }
}
