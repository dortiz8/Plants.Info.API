using System;
using Plants.info.API.Data.Repository;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.UserServices
{
	public class UserService : IUserService
	{
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
		{
            _userRepository = userRepository;
        }

        public Task<PlantInfoUser?> GetUserByIdAsync(int userId)
        {
            return _userRepository.GetUserByIdAsync(userId); 
        }

        public Task<bool> SaveAllChangesAsync()
        {
            return _userRepository.SaveAllChangesAsync(); 
        }

        public Task<bool> UserExistsAsync(int userId)
        {
            return _userRepository.UserExistsAsync(userId); 
        }
    }
}

