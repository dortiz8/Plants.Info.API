using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public interface IUserRepository : IDbActions
    {
        Task<IEnumerable<PlantInfoUser>> GetAllUsersAsync();
        Task<PlantInfoUser?> GetUserByIdAsync(int Id);

        Task<bool> UserExistsAsync(int Id);
        Task<bool> UserNameExistsAsync(string userName); 

        Task<bool> ValidateUserAsync(string userName, string password);

        Task<PlantInfoUser?> FindUserByUsernameAsync(string userName);
        Task<PlantInfoUser?> FindUserByEmailAsync(string email);


        Task CreateUserAsync(PlantInfoUser userObject);

        Task DeleteUserAsync(int Id); 
        
    }
}
