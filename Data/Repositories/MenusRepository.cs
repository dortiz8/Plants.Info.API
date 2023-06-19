using System;
using Microsoft.EntityFrameworkCore;
using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;

namespace Plants.info.API.Data.Services
{
	public class MenusRepository : IMenusRepository
	{
        private readonly UserContext _ctx;

        public MenusRepository(UserContext userContext)
        {
            _ctx = userContext;
        }
        

        public async Task<IEnumerable<Genus>> getGenusList()
        {
            return await _ctx.Genus.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<Genus?> getGenusById(int genusId)
        {
            return await _ctx.Genus.Where(g => g.Id == genusId).FirstOrDefaultAsync(); 
        }
    }
}

