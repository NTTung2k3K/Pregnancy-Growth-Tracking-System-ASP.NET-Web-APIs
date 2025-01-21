using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Request;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.RoleModelViews;
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


            CreateMap<ApplicationRoles, RoleModelView>().ReverseMap();
            CreateMap<ApplicationRoles, CreateRoleModelView>();
            CreateMap<ApplicationRoles, UpdatedRoleModelView>();
            CreateMap<CreateRoleModelView, ApplicationRoles>();
            CreateMap<UpdatedRoleModelView, ApplicationRoles>();



        }
    }
}
