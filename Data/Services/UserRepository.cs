using Microsoft.EntityFrameworkCore;
using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _ctx;

        public UserRepository(UserContext userContext)
        {
            _ctx = userContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
           return await _ctx.Users.OrderBy(x => x.UserName).ToListAsync(); 
        }

        public  async Task<User?> GetUserByIdAsync(int Id)
        {
            return await _ctx.Users.FirstOrDefaultAsync(x => x.Id == Id); 
        }

        public async Task<bool> SaveAllChangesAsync()
        {
            return (await _ctx.SaveChangesAsync() >= 0); 
        }

        public async Task<bool> UserExistsAsync(int Id)
        {
            return await _ctx.Users.AnyAsync(x => x.Id == Id); 
        }
    }
}
