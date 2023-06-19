using System;
using Microsoft.AspNetCore.JsonPatch;
using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.PlantServices.Interfaces
{
	public interface IPlantService
	{
        Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id);
        Task<Plant> CreatePlantAsync(int userId, PlantCreation plantObject);
        Task DeletePlantAsync(int userId, int id);
        Task<Boolean> DoesPlantExists(int userId, string name, int genus);
        Task<PlantInfo?> GetSinglePlantByIdWithInfoAsync(int userId, Plant plant);
        Task UpdatePlantAsync(int userId, Plant plant, PlantEdit plantObject);
        Task<PlantInfo> PatchPlantAsync(int userId, Plant plant, PlantCreation patchDocument);
        Task PatchPlantImageAsync(int userId, PlantImageEdit plantImage);
        Task<PlantCreation> MapPlantToPatch(int userId, Plant plant);

        Task<(IEnumerable<PlantNote>, PaginationMetadata)> GetPlantNotesAsync(int userId, int plantId, int pageNumber, int pageSize);
        Task<PlantNote?> GetSinglePlantNoteAsync(int userId, int plantId, int noteId);
        Task DeletePlantNoteAsync(int userId, int plantId, int noteId);
        Task<PlantNote?> CreatePlantNoteAsync(int userId, int plantId, PlantNoteCreation plantNoteObject);

        Task PatchPlantNoteAsync(PlantNote plantNote, PlantNoteCreation plantNoteToPatch); 
    }
}

