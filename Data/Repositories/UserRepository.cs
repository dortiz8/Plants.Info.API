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

        public async Task CreateUserAsync(PlantInfoUser userObject)
        {
            await _ctx.Users.AddAsync(userObject); 
        }

        public async Task DeleteUserAsync(int Id)
        {
            var user = await GetUserByIdAsync(Id);
            if(user != null)
            {
                _ctx.Users.Remove(user);
            }
        }

        public async Task<PlantInfoUser?> FindUserByUsernameAsync(string userName)
        {
            return await _ctx.Users.FirstOrDefaultAsync(x => x.UserName == userName); 
        }

        public async Task<PlantInfoUser?> FindUserByEmailAsync(string email)
        {
            return await _ctx.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IEnumerable<PlantInfoUser>> GetAllUsersAsync()
        {
           return await _ctx.Users.OrderBy(x => x.UserName).ToListAsync(); 
        }

        public  async Task<PlantInfoUser?> GetUserByIdAsync(int Id)
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

        public async Task<bool> UserNameExistsAsync(string userName)
        {
            return await _ctx.Users.AnyAsync(x => x.UserName == userName); 
        }

        public Task<bool> ValidateUserAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
