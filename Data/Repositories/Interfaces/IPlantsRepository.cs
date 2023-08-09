using Plants.info.API.Data.Models;
using Plants.info.API.Data.Services;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Repository
{
    public interface IPlantsRepository : IDbActions
    {
        Task<IEnumerable<Plant>> GetAllPlantsAsync();
        Task<IEnumerable<Plant>> GetPlantsByIdAsync(int Id);
        Task<(IEnumerable<Plant>, PaginationMetadata)> GetPlantsByIdAsync(int Id, string? name, string? queryString, int pageNumber, int pageSize);
        Task<PlantsStats> GetPlantsStatsByIdAsync(int Id);
        Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id);

        Task CreatePlantAsync(Plant plantObject);
        //Task UpdatePlantAsync(Plant plant); 

        Task DeletePlantAsync(int userId, int id);
        Task<Boolean> DoesPlantExists(int userId, string name, int genus);
        Task<(IEnumerable<PlantNote>, PaginationMetadata)> GetPlantNotesAsync(int userId, int plantId, int pageNumber, int pageSize);
        Task<PlantNote?> GetSinglePlantNoteAsync(int userId, int plantId, int noteId);
        Task DeletePlantNoteAsync(int userId, int plantId, int noteId);
        Task CreatePlantNoteAsync(PlantNote plantNoteObject);

        Task DeleteAllPlantNotesByUserIdAsync(int userId, int plantId); 

        Task<PlantImage?> GetPlantImage(int userId, int plantId);
        Task CreatePlantImageAsync(PlantImage plantImage); 

    }
}
