using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Services.Interface;
using BabyCare.Repositories.Context;
using BabyCare.Services;
using BabyCare.Services.Service;
using BabyCare.Repositories.Mapper;

namespace BabyCare.API
{
    public static class DependencyInjection
    {
        public static void AddConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigRoute();
            services.AddDatabase(configuration);
            services.AddIdentity();
            services.AddInfrastructure(configuration);
            services.AddServices();
            services.AddAutoMapperProfiles();
        }

        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }

        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("BabyCareDb"));
            });
        }

        public static void AddIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUsers, ApplicationRoles>(options =>
            {
                // Identity configuration options
            })
             .AddEntityFrameworkStores<DatabaseContext>()
             .AddDefaultTokenProviders();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IRoleService, RoleService>();
        }

        public static void AddAutoMapperProfiles(this IServiceCollection services)
        {
            // Register AutoMapper and scan for profiles in the assembly
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        }
    }
}
