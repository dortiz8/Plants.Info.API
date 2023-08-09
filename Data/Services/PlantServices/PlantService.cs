using System;
using System.Buffers.Text;
using System.Drawing;
using System.Xml.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualBasic;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Data.Services.PlantServices.Interfaces;
using Plants.info.API.Data.Services.Utils;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.PlantServices
{
	public class PlantService : IPlantService

	{
        private readonly IPlantsRepository _plantsRepo;
        private readonly IMenusRepository _menusRepository;

        public PlantService(IPlantsRepository plantsRepo, IMenusRepository menusRepository)
		{
            _plantsRepo = plantsRepo;
            _menusRepository = menusRepository;
        }

        public async Task<Plant> CreatePlantAsync(int userId, PlantCreation plantObject)
        {
            var finalPlant = new Plant()
            {
                Name = plantObject.Name,
                GenusId = plantObject.GenusId,
                DateAdded = DateTime.Now,
                DateWatered = Convert.ToDateTime(plantObject.DateWatered),
                DateFertilized = Convert.ToDateTime(plantObject.DateFertilized),
                WaterInterval = plantObject.WaterInterval,
                FertilizeInterval = plantObject.FertilizeInterval,
                UserId = userId,

            };

            await _plantsRepo.CreatePlantAsync(finalPlant);
            await _plantsRepo.SaveAllChangesAsync();

            if (plantObject.Image != null)
            {
                var plantImage = new PlantImage()
                {
                    UserId = plantObject.Image.UserId,
                    PlantId = finalPlant.Id,
                    Name = plantObject.Image.Name,
                    Type = plantObject.Image.Type,
                    Size = plantObject.Image.Size,
                    Base64 = plantObject.Image.Base64

                };
                await _plantsRepo.CreatePlantImageAsync(plantImage);
                await _plantsRepo.SaveAllChangesAsync();
            }

            return finalPlant; 
        }

        public async Task DeletePlantAsync(int userId, int plantId)
        {
            await _plantsRepo.DeletePlantAsync(userId, plantId);
            await _plantsRepo.DeleteAllPlantNotesByUserIdAsync(userId, plantId); 
            await _plantsRepo.SaveAllChangesAsync();
        }

        public async Task<bool> DoesPlantExists(int userId, string name, int genus)
        {
            return await _plantsRepo.DoesPlantExists(userId, name, genus); 
        }

        public async Task<Plant?> GetSinglePlantByIdAsync(int userId, int Id)
        {
            return await _plantsRepo.GetSinglePlantByIdAsync(userId, Id);
        }

        public async Task<PlantInfo?> GetSinglePlantByIdWithInfoAsync(int userId, Plant plant)
        {
            var genusName = await _menusRepository.getGenusById(plant.GenusId);
            var image = await _plantsRepo.GetPlantImage(userId, plant.Id);

            var plantInfo = new PlantInfo()
            {
                Id = plant.Id,
                Name = plant.Name,
                GenusId = plant.GenusId,
                GenusName = genusName != null ? genusName.Name : "",
                DateAdded = plant.DateAdded,
                DateWatered = Convert.ToDateTime(plant.DateWatered),
                DateFertilized = Convert.ToDateTime(plant.DateFertilized),
                WaterInterval = plant.WaterInterval,
                FertilizeInterval = plant.FertilizeInterval,
                UserId = userId,
                Image = image
            };

            return plantInfo; 
        }

        public async Task<PlantCreation> MapPlantToPatch(int userId, Plant plant)
        {
            PlantImageEdit? editPlantImage = null; 

            // Only attempt to map image if the patch document includes an image
            var plantImage = await _plantsRepo.GetPlantImage(userId, plant.Id);


            if (plantImage != null)
            {
                editPlantImage = new PlantImageEdit(); 
                editPlantImage.UserId = plantImage.UserId;
                editPlantImage.PlantId = plantImage.PlantId;
                editPlantImage.Name = plantImage.Name;
                editPlantImage.Type = plantImage.Type;
                editPlantImage.Size = plantImage.Size;
                editPlantImage.Base64 = plantImage.Base64;
            }

            // Map it to a PlantCreation model
            return new PlantCreation()
            {
                Name = plant.Name,
                GenusId = plant.GenusId,
                DateAdded = plant.DateAdded,
                DateWatered = plant.DateWatered,
                DateFertilized = plant.DateFertilized,
                WaterInterval = plant.WaterInterval,
                FertilizeInterval = plant.FertilizeInterval,
                Image = editPlantImage
            };
        }

        public async Task<PlantInfo> PatchPlantAsync(int userId, Plant plant, PlantCreation plantToPatch)
        {
            plant.Name = plantToPatch.Name;
            plant.GenusId = plantToPatch.GenusId;
            plant.DateAdded = plantToPatch.DateAdded;
            plant.DateWatered = plantToPatch.DateWatered;
            plant.DateFertilized = plantToPatch.DateFertilized;
            plant.WaterInterval = plantToPatch.WaterInterval;
            plant.FertilizeInterval = plantToPatch.FertilizeInterval;

            await _plantsRepo.SaveAllChangesAsync();

            var genusName = await _menusRepository.getGenusById(plant.GenusId);
            var image = await _plantsRepo.GetPlantImage(userId, plant.Id);

            var plantInfo = new PlantInfo()
            {
                Id = plant.Id,
                Name = plant.Name,
                GenusId = plant.GenusId,
                GenusName = genusName != null ? genusName.Name : "",
                DateAdded = plant.DateAdded,
                DateWatered = Convert.ToDateTime(plant.DateWatered),
                DateFertilized = Convert.ToDateTime(plant.DateFertilized),
                WaterInterval = plant.WaterInterval,
                FertilizeInterval = plant.FertilizeInterval,
                UserId = userId,
                Image = image
            };

            return plantInfo;
        }

        public async Task UpdatePlantAsync(int userId, Plant plant, PlantEdit plantObject)
        {
            plant.Name = plantObject.Name;
            plant.GenusId = plantObject.GenusId;
            plant.DateWatered = plantObject.DateWatered;
            plant.DateFertilized = plantObject.DateFertilized;
            plant.WaterInterval = plantObject.WaterInterval;
            plant.FertilizeInterval = plantObject.FertilizeInterval;


            if (plantObject.Image != null)
            {
                var plantImage = await _plantsRepo.GetPlantImage(userId, plant.Id);
                if (plantImage != null)
                {
                    plantImage.UserId = plantObject.Image.UserId;
                    plantImage.PlantId = plantObject.Image.PlantId;
                    plantImage.Name = plantObject.Image.Name;
                    plantImage.Type = plantObject.Image.Type;
                    plantImage.Size = plantObject.Image.Size;
                    plantImage.Base64 = plantObject.Image.Base64; 
                    //plantImage = new PlantImage()
                    //{
                    //    UserId = plantObject.Image.UserId,
                    //    PlantId = plantObject.Image.PlantId,
                    //    Name = plantObject.Image.Name,
                    //    Type = plantObject.Image.Type,
                    //    Size = plantObject.Image.Size,
                    //    Base64 = plantObject.Image.Base64

                    //};
                }
                else
                {
                    var newPlantImage = new PlantImage()
                    {
                        UserId = plantObject.Image.UserId,
                        PlantId = plantObject.Image.PlantId,
                        Name = plantObject.Image.Name,
                        Type = plantObject.Image.Type,
                        Size = plantObject.Image.Size,
                        Base64 = plantObject.Image.Base64

                    };
                    await _plantsRepo.CreatePlantImageAsync(newPlantImage);
                }

            }
            await _plantsRepo.SaveAllChangesAsync();
        }

        public Task PatchPlantImageAsync(int userId, PlantImageEdit plantImage)
        {

            // ***** TO DO implement image patching and saving

            //PlantImage? plantImage = null; 

            //if (plantToPatch.Image != null)
            //{
            //    plantImage = await _plantsRepo.GetPlantImage(userId, plant.Id);

            //    if(plantImage != null)
            //    {
            //        plantImage.UserId = plantToPatch.Image.UserId;
            //        plantImage.PlantId = plantToPatch.Image.PlantId;
            //        plantImage.Name = plantToPatch.Image.Name;
            //        plantImage.Type = plantToPatch.Image.Type;
            //        plantImage.Size = plantToPatch.Image.Size;
            //        plantImage.Base64 = plantToPatch.Image.Base64; 
            //    }

            //    plantImage = new PlantImage()
            //    {
            //        UserId = plantToPatch.Image.UserId,
            //        PlantId = plantToPatch.Image.PlantId,
            //        Name = plantToPatch.Image.Name,
            //        Type = plantToPatch.Image.Type,
            //        Size = plantToPatch.Image.Size,
            //        Base64 = plantToPatch.Image.Base64,
            //    };
            //};

            throw new NotImplementedException();
        }

        public async Task<(IEnumerable<PlantNote>, PaginationMetadata)> GetPlantNotesAsync(int userId, int plantId, int pageNumber, int pageSize)
        {
            return await _plantsRepo.GetPlantNotesAsync(userId, plantId, pageNumber, pageSize);
        }

        public async Task<PlantNote?> GetSinglePlantNoteAsync(int userId, int plantId, int noteId)
        {
            return await _plantsRepo.GetSinglePlantNoteAsync(userId, plantId, noteId);
        }

        public async Task DeletePlantNoteAsync(int userId, int plantId, int noteId)
        {
            await _plantsRepo.DeletePlantNoteAsync(userId, plantId, noteId);

            await _plantsRepo.SaveAllChangesAsync();
        }

        public async Task<PlantNote?> CreatePlantNoteAsync(int userId, int plantId, PlantNoteCreation plantNoteObject)
        {
            var finalPlantNote = new PlantNote()
            {
                UserId = userId,
                PlantId = plantId,
                Description = plantNoteObject.Description,
                DateAdded = DateTime.Now,
                DateEdited = null
            };

            await _plantsRepo.CreatePlantNoteAsync(finalPlantNote);
            await _plantsRepo.SaveAllChangesAsync();

            return finalPlantNote; 
        }

        public async Task PatchPlantNoteAsync(PlantNote plantNote, PlantNoteCreation plantNoteToPatch)
        {
            plantNote.Description = plantNoteToPatch.Description;
            plantNote.DateEdited = DateAndTime.Now;

            await _plantsRepo.SaveAllChangesAsync();
        }
    }
}

