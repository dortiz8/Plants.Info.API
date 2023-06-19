using System;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Data.Services.PlantServices.Interfaces;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.PlantServices
{
	public class PlantsService : IPlantsService
	{
        private readonly IPlantsRepository _plantsRepo;
        private readonly IUserRepository _userRepo;
        public PlantsService(IPlantsRepository plantsRepo, IUserRepository userRepo)
		{
            _plantsRepo = plantsRepo;
            _userRepo = userRepo;
        }

        public async Task<(IEnumerable<Plant>, PaginationMetadata)> GetPlantsById(int userId, string? name, string? queryString, int pageNumber, int pageSize)
        {
            var (plants, paginationMetadata) = await _plantsRepo.GetPlantsByIdAsync(userId, name, queryString, pageNumber, pageSize);
            return (plants, paginationMetadata); 
        }

        public async Task<PlantsStats> GetPlantsStatsByIdAsync(int userId)
        {
            return await _plantsRepo.GetPlantsStatsByIdAsync(userId);
        }
    }
}

