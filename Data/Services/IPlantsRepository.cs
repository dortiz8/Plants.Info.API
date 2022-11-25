using Plants.info.API.Data.Services;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public interface IPlantsRepository : IDbActions
    {
        Task<IEnumerable<Plant>> GetAllPlantsAsync();
        Task<IEnumerable<Plant>> GetPlantsByIdAsync(int Id);
        Task<(IEnumerable<Plant>, PaginationMetadata)> GetPlantsByIdAsync(int Id, string? name, string? queryString, int pageNumber, int pageSize); 
        Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id);

        Task CreatePlantAsync(Plant plantObject);
        //Task UpdatePlantAsync(Plant plant); 

        Task DeletePlantAsync(int userId, int id);    
    }
}
