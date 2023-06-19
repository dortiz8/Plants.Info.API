using System;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using Plants.info.API.Models;
using Plants.info.API.Data.Models;

namespace Plants.info.API.Data.Services.PlantServices.Interfaces
{
    public interface IPlantsService
    {
       Task<(IEnumerable<Plant>, PaginationMetadata)> GetPlantsById(int userId, string? name, string? queryString, int pageNumber, int pageSize);
       Task<PlantsStats> GetPlantsStatsByIdAsync(int userId); 
    }
}

