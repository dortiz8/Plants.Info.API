using System;
using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.Utils
{
	public static class MapService
	{
		public static PlantCreation MapToPlantCreation(Plant plant)
		{
			return new PlantCreation()
            {
                Name = plant.Name,
                GenusId = plant.GenusId,
                DateAdded = plant.DateAdded,
                DateWatered = plant.DateWatered,
                DateFertilized = plant.DateFertilized,
                WaterInterval = plant.WaterInterval,
                FertilizeInterval = plant.FertilizeInterval,
            };
        }

        public static Plant MapToPlantFromCreation(Plant plant,  PlantCreation plantToPatch)
        {
            plant.Name = plantToPatch.Name;
            plant.GenusId = plantToPatch.GenusId;
            plant.DateAdded = plantToPatch.DateAdded;
            plant.DateWatered = plantToPatch.DateWatered;
            plant.DateFertilized = plantToPatch.DateFertilized;
            plant.WaterInterval = plantToPatch.WaterInterval;
            plant.FertilizeInterval = plantToPatch.FertilizeInterval;

            return plant; 
        }

        public static PlantNoteCreation MapToPlantNoteCreation(PlantNote plantNote)
        {
            return new PlantNoteCreation()
            {
                Description = plantNote.Description
            };
        }
	}
}

