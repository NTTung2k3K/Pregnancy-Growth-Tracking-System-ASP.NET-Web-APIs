using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Request;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.BlogModelViews;
using BabyCare.ModelViews.BlogTypeModelView;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.FeedbackModelView;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using BabyCare.ModelViews.FetalGrowthStandardModelView;
using BabyCare.ModelViews.GrowthChartModelView;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.PaymentModelView.Response;
using BabyCare.ModelViews.RoleModelViews;
using BabyCare.ModelViews.UserMembershipModelView.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;

namespace BabyCare.Repositories.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region user
            CreateMap<ApplicationUsers, UserLoginResponseModel>().ReverseMap();
            CreateMap<ApplicationUsers, UserResponseModel>().ReverseMap();
            CreateMap<ApplicationUsers, UpdateUserProfileRequest>().ReverseMap();


            #endregion
            #region employee
            CreateMap<EmployeeLoginResponseModel, ApplicationUsers>().ReverseMap();

            CreateMap<CreateEmployeeRequest, ApplicationUsers>().ReverseMap();
            CreateMap<UpdateEmployeeProfileRequest, ApplicationUsers>().ReverseMap();
            CreateMap<ApplicationUsers, EmployeeResponseModel>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRoles.FirstOrDefault().Role)) 
                .ReverseMap()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            CreateMap<ApplicationRoles, RoleModelView>()
                .ReverseMap();
            #endregion
            #region Membership Package
            CreateMap<CreateMPRequest, MembershipPackage>().ReverseMap();
            CreateMap<UpdateMPRequest, MembershipPackage>().ReverseMap();
            CreateMap<MPResponseModel, MembershipPackage>().ReverseMap();

            #endregion
            #region AppointmentTemplates
            CreateMap<CreateATRequest, AppointmentTemplates>().ReverseMap();
            CreateMap<UpdateATRequest, AppointmentTemplates>().ReverseMap();
            CreateMap<ATResponseModel, AppointmentTemplates>().ReverseMap();
            #endregion
            #region Appointments
            CreateMap<CreateAppointmentRequest, Appointment>().ReverseMap();
            CreateMap<UpdateAppointmentRequest, Appointment>().ReverseMap();
            CreateMap<AppointmentResponseModel, Appointment>().ReverseMap();
            CreateMap<AppointmentResponseModelV2, Appointment>().ReverseMap();
            CreateMap<AppoinmentUserResponseModel, AppointmentUser>().ReverseMap();

            // Add
            CreateMap<ChildModelView, Child>().ReverseMap();
            CreateMap<ChildModelViewAddeRecords, Child>().ReverseMap();
            CreateMap<FetalGrowthStandardModelView, FetalGrowthStandard>().ReverseMap();
            CreateMap<FetalGrowthRecordModelViewAddedStandards, FetalGrowthRecord>().ReverseMap();
            CreateMap<Child, ChildModelViewAddeRecords>();

            // Mapping cho FetalGrowthRecord
            CreateMap<FetalGrowthRecord, FetalGrowthRecordModelViewAddedStandards>()
                .ForMember(dest => dest.FetalGrowthStandardModelView,
                           opt => opt.MapFrom(src => src.FetalGrowthStandard));

            // Mapping cho FetalGrowthStandard
            CreateMap<FetalGrowthStandard, FetalGrowthStandardModelView>();

            #endregion

            #region Record
            CreateMap<FetalGrowthRecord, CreateFetalGrowthRecordModelView>().ReverseMap();
            #endregion

            #region Payment 
            CreateMap<Payment,PaymentResponseModel>().ReverseMap();
            #endregion
            #region Usermembership
            CreateMap<UserMembership, UserMembershipResponse>().ReverseMap();
            #endregion


            CreateMap<ApplicationRoles, RoleModelView>().ReverseMap();
            CreateMap<ApplicationRoles, CreateRoleModelView>().ReverseMap();
            CreateMap<ApplicationRoles, UpdatedRoleModelView>().ReverseMap();


            CreateMap<BlogType, BlogTypeModelView>().ReverseMap();
            CreateMap<BlogType, CreateBlogTypeModelView>().ReverseMap();
            CreateMap<BlogType, UpdateBlogTypeModelView>().ReverseMap();

            CreateMap<Blog, BlogModelView>().ReverseMap();
            CreateMap<Blog, CreateBlogModelView>().ReverseMap();
            CreateMap<Blog, UpdateBlogModelView>().ReverseMap();

            CreateMap<Child, ChildModelView>().ReverseMap();
            CreateMap<Child, CreateChildModelView>().ReverseMap();
            CreateMap<Child, UpdateChildModelView>().ReverseMap();

            CreateMap<GrowthChart, GrowthChartModelView>().ReverseMap();
            CreateMap<GrowthChart, CreateGrowthChartModelView>().ReverseMap();
            CreateMap<GrowthChart, UpdateGrowthChartModelView>().ReverseMap();
            CreateMap<GrowthChart, UpdateGrowChartByAdmin>().ReverseMap();
            CreateMap<GrowthChart, UpdateGrowChartByUser>().ReverseMap();


            CreateMap<Feedback, FeedbackModelView>().ReverseMap();
            
            CreateMap<Feedback, FeedbackModelViewForAdmin>().ReverseMap();

            CreateMap<Feedback, CreateFeedbackModelView>().ReverseMap();
            CreateMap<Feedback, UpdateFeedbackModelView>().ReverseMap();


            CreateMap<FetalGrowthStandard, FetalGrowthStandardModelView>().ReverseMap();
            CreateMap<FetalGrowthStandard, CreateFetalGrowthStandardModelView>().ReverseMap();
            CreateMap<FetalGrowthStandard, UpdateFetalGrowthStandardModelView>().ReverseMap();
        }
    }
}
