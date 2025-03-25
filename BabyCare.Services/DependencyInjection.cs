using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Repositories.UOW;

namespace BabyCare.Services
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositories();
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
