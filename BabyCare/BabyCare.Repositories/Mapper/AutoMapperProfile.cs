using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.ModelViews.RoleModelViews;
using BabyCare.ModelViews.UserModelViews.Response;

namespace BabyCare.Repositories.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region user
            CreateMap<ApplicationUsers,UserLoginResponseModel>().ReverseMap();
            #endregion



            CreateMap<ApplicationRoles, RoleModelView>();
			CreateMap<ApplicationRoles, CreateRoleModelView>();
			CreateMap<ApplicationRoles, UpdatedRoleModelView>();
			CreateMap<CreateRoleModelView, ApplicationRoles>();
			CreateMap<UpdatedRoleModelView, ApplicationRoles>();
   
        }
    }
}
