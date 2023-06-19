using System;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.UserServices
{
	public interface IUserService
	{
        Task<bool> UserExistsAsync(int userId);
        Task<PlantInfoUser?> GetUserByIdAsync(int userId);
        Task<bool> SaveAllChangesAsync(); 
    }
}

