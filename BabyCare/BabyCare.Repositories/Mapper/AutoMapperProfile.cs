using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.ModelViews.RoleModelViews;

namespace BabyCare.Repositories.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            
			CreateMap<ApplicationRoles, RoleModelView>();
			CreateMap<ApplicationRoles, CreateRoleModelView>();
			CreateMap<ApplicationRoles, UpdatedRoleModelView>();
			CreateMap<CreateRoleModelView, ApplicationRoles>();
			CreateMap<UpdatedRoleModelView, ApplicationRoles>();
   
        }
    }
}
