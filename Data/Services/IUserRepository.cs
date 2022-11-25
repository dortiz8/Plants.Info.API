using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public interface IUserRepository : IDbActions
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int Id);

        Task<bool> UserExistsAsync(int Id); 
    }
}
