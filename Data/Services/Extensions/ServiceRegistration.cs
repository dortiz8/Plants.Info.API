using System;
using Plants.info.API.Data.Services.JwtFeatures;
using Plants.info.API.Data.Services.PlantServices;
using Plants.info.API.Data.Services.PlantServices.Interfaces;
using Plants.info.API.Data.Services.UserServices;

namespace Plants.info.API.Data.Services.Extensions
{
	public static class ServiceRegistration
	{

		public static IServiceCollection MyServicesGroup(
			 this IServiceCollection services)
		{
            services.AddScoped<IPlantService, PlantService>();
            services.AddScoped<IPlantsService, PlantsService>();
            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddScoped<IUserService, UserService>();

            return services; 
        }
	}
}; 

