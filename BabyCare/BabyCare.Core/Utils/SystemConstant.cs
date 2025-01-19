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
        public class MembershipPackageStatus
        {
            public static string Active = "Active";
            public static string InActive = "InActive";

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
