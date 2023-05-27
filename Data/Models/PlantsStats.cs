using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plants.info.API.Data.Models
{
	public class PlantsStats
	{
		public int totalPlants { get; set; }
        public int totalPlantsThatNeedWatering { get; set; }
        public int totalPlantsThatNeedFertilizing { get; set; }
        public List<GenusStat>? genusList { get; set; }

    }

    [Keyless]
    public class GenusStat
    {
        public int genusId { get; set; }
        public string? genusName { get; set; }
        public int total { get; set; }
    }

}


