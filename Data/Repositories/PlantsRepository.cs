using Microsoft.EntityFrameworkCore;
using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;
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

        public async Task<bool> DoesPlantExists(int userId, string name, int genus)
        {
            var count = await _ctx.Plants.CountAsync(x => x.UserId == userId && x.Name == name && x.GenusId == genus);
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
                // Figure how to filter by Genus
                //collection = collection.Where(x => (x.UserId == userId) && (x.Name.Contains(queryString) || (x.Genus != null && x.Genus.Contains(queryString))));
            }

            // Begin constructing Pagination metadata
            var paginationTotalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(paginationTotalItemCount, pageSize, pageNumber); 

            var collectionToReturn = await collection.OrderBy(x => x.Name).Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync(); // Wait until paging because we want to apply it on the filtered and searched data.
            return (collectionToReturn, paginationMetadata); 
        }

        public async Task<PlantsStats> GetPlantsStatsByIdAsync(int userId)
        {
            var collection = _ctx.Plants as IQueryable<Plant>;
            collection = collection.Where(x => x.UserId == userId);

            var plantsStats = new PlantsStats() { };

            if(collection != null)
            {
                plantsStats.totalPlants = await collection.CountAsync();
                plantsStats.totalPlantsThatNeedWatering = await _ctx.Plants.FromSqlInterpolated($"SELECT * FROM dbo.Plants p WHERE p.UserId = {userId} AND DATEDIFF(day, p.DateWatered, GETDATE()) > p.WaterInterval")
                    .CountAsync();
                plantsStats.totalPlantsThatNeedFertilizing = await _ctx.Plants.FromSqlInterpolated($"SELECT * FROM dbo.Plants p WHERE p.UserId = {userId} AND DATEDIFF(day, p.DateFertilized, GETDATE()) > p.FertilizeInterval")
                    .CountAsync();
                plantsStats.genusList = await _ctx.GenusStat.FromSqlInterpolated($"SELECT DISTINCT GenusId AS genusId, g.Name AS genusName, (SELECT COUNT(*) from dbo.Plants o WHERE GenusId = p.GenusId AND o.UserId = {userId}) AS total FROM dbo.Plants p INNER JOIN dbo.Genus g ON g.Id = p.GenusId WHERE UserId = {userId}")
                    .ToListAsync(); 
            }

           // Wait until paging because we want to apply it on the filtered and searched data.
            return plantsStats;
        }

        public async Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id)
        {
            return await _ctx.Plants.Where(x => x.UserId == userId && x.Id == Id).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAllChangesAsync()
        {
            return (await _ctx.SaveChangesAsync() >= 0);
        }

        public async Task<(IEnumerable<PlantNote>, PaginationMetadata)> GetPlantNotesAsync(int userId, int plantId, int pageNumber, int pageSize)
        {
            var collection = _ctx.PlantNotes as IQueryable<PlantNote>;
            collection = collection.Where(x => x.UserId == userId && x.PlantId == plantId);

            // Pagination
            var paginationTotalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(paginationTotalItemCount, pageSize, pageNumber);
            //Filtering
            var collectionToReturn = await collection.OrderBy(x => x.DateEdited).Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync(); // Wait until paging because we want to apply it on the filtered and searched data.

            return (collectionToReturn, paginationMetadata);
        }

        public async Task DeletePlantNoteAsync(int userId, int plantId, int noteId)
        {
            var noteToRemove = await _ctx.PlantNotes.Where(x => x.UserId == userId && x.PlantId == plantId && x.Id == noteId).FirstOrDefaultAsync();

            if(noteToRemove != null)
            {
                _ctx.PlantNotes.Remove((PlantNote)noteToRemove); 
            }
            
        }

        public async Task CreatePlantNoteAsync(PlantNote plantNoteObject)
        {
            await _ctx.PlantNotes.AddAsync(plantNoteObject);
        }

        public async Task<PlantNote?> GetSinglePlantNoteAsync(int userId, int plantId, int noteId)
        {
           return await _ctx.PlantNotes.Where(x => x.UserId == userId && x.PlantId == plantId && x.Id == noteId).FirstOrDefaultAsync();
        }

        public async Task DeleteAllPlantNotesByUserIdAsync(int userId, int plantId)
        {
            var notesToRemove = await _ctx.PlantNotes.Where(x => x.UserId == userId && x.PlantId == plantId).ToListAsync();
            if(notesToRemove != null)
            {
                _ctx.PlantNotes.RemoveRange(notesToRemove); 
            }
        }

        public async Task<PlantImage?> GetPlantImage(int userId, int plantId)
        {
            return await _ctx.PlantImage.Where(x => x.UserId == userId && x.PlantId == plantId).FirstOrDefaultAsync(); 
        }

        public async Task CreatePlantImageAsync(PlantImage plantImage)
        {
            await _ctx.PlantImage.AddAsync(plantImage); 
        }

    }
}
