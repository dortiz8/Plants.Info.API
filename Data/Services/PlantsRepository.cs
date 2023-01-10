using Microsoft.EntityFrameworkCore;
using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Services;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public class PlantsRepository : IPlantsRepository
    {
        private readonly UserContext _ctx;

        public PlantsRepository(UserContext userContext)
        {
            _ctx = userContext;
        }

        public async Task CreatePlantAsync(Plant plantObject)
        {
             await _ctx.Plants.AddAsync(plantObject);
        }

        public async Task DeletePlantAsync(int userId, int Id)
        {

            var plantToRemove = await GetSinglePlantByIdAsync(userId, Id);

            if (plantToRemove != null)
            {
                _ctx.Plants.Remove((Plant)plantToRemove);  
            }
        }

        public async Task<bool> DoesPlantExists(int userId, string name, string genus)
        {
            var count = await _ctx.Plants.CountAsync(x => x.UserId == userId && x.Name == name && x.Genus == genus);
            return (count > 0); 
        }

        public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
        {
            return await _ctx.Plants.OrderBy(x => x.UserId).ToListAsync();
        }
        public async Task<IEnumerable<Plant>> GetPlantsByIdAsync(int Id)
        {
            return await _ctx.Plants.Where(x => x.UserId == Id).ToListAsync();
        }
        public async Task<(IEnumerable<Plant>, PaginationMetadata)> GetPlantsByIdAsync(int userId, string? name, string? queryString, int pageNumber, int pageSize)
        {
            //if (string.IsNullOrEmpty(name) && string.IsNullOrWhiteSpace(queryString)) // We need to apply paging no matter what. 
            //{
            //    return await GetPlantsByIdAsync(userId);
            //}
            var collection = _ctx.Plants as IQueryable<Plant>;
             collection = collection.Where(x => x.UserId == userId);

            // Unsure as to what this line is looking for? 
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(x => x.UserId == userId && x.Name == name);
            }
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                queryString = queryString.Trim();
                collection = collection.Where(x => (x.UserId == userId) && (x.Name.Contains(queryString) || (x.Genus != null && x.Genus.Contains(queryString))));
            }

            // Begin constructing Pagination metadata
            var paginationTotalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(paginationTotalItemCount, pageSize, pageNumber); 

            var collectionToReturn = await collection.OrderBy(x => x.Name).Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync(); // Wait until paging because we want to apply it on the filtered and searched data.
            return (collectionToReturn, paginationMetadata); 
           
        }

        public async Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id)
        {
            return await _ctx.Plants.Where(x => x.UserId == userId && x.Id == Id).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAllChangesAsync()
        {
            return (await _ctx.SaveChangesAsync() >= 0);
        }

        //public async Task UpdatePlantAsync(Plant plantObject)
        //{
        //    return await _ctx.Plants.Update(plantObject);
        //}
    }
}
