using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Models;
using System.Xml.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Plants.info.API.Data.Services;

namespace Plants.info.API.Controllers
{
    [Route("api/users/{userId}/plants")] // Since we need to gather user information to get plants we reflect the URL as such and set plants as a child resource of Users. 
    [Authorize] //we have no way to pass down id values to this policy, therefore a custom attribute needs to be created but it is not recommended.
    [ApiController]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantsRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<PlantsController> _log;
        private readonly IMenusRepository _menusRepository;
        const int maxPageSizeForPlants = 20; 
        public PlantsController(IPlantsRepository repo, IUserRepository userRepo, ILogger<PlantsController> logger, IMenusRepository menusRepository)
        {
            _repo = repo;
            _userRepo = userRepo;
            _log = logger ?? throw new ArgumentNullException(nameof(logger)); // Null check in case the container changes and it returns a null value
            _menusRepository = menusRepository;
        }


        [HttpGet] // This is sufficient since we defined the templete at the controller level. 
        public async Task<ActionResult<IEnumerable<Plant>>> GetPlantsByIdAsync(int userId, [FromQuery] string? name, 
                                                                                [FromQuery] string? queryString, [FromQuery] int pageNumber = 1, 
                                                                                [FromQuery] int pageSize = 10)
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if(IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                // Default pagination
                if (pageSize > maxPageSizeForPlants)
                {
                    pageSize = maxPageSizeForPlants; 
                }

                if (!await _userRepo.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var (plants, paginationMetadata) = await _repo.GetPlantsByIdAsync(userId, name, queryString, pageNumber, pageSize);

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata)); 

                return Ok(plants);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex); 
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

       

        [HttpGet("{plantId}", Name = "GetPlantById")] // Name the method to easily refer to it when calling into CreatedAtRoute. 
        public async Task<ActionResult<Plant>> GetPlantByIdAsync(int userId, int plantId, [FromQuery] bool info = false) // Make sure parameter names match when dealing with multiple parameters. 
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId);
                if (!user) return NotFound();
                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                if (info)
                {
                    var genusName = await _menusRepository.getGenusById(plant.GenusId); 
                    var plantInfo = new PlantInfo()
                    {
                        Id = plant.Id,
                        Name = plant.Name,
                        GenusId = plant.GenusId,
                        GenusName = genusName != null ? genusName.Name : "", 
                        DateAdded = DateTime.Now,
                        DateWatered = Convert.ToDateTime(plant.DateWatered),
                        DateFertilized = Convert.ToDateTime(plant.DateFertilized),
                        WaterInterval = plant.WaterInterval,
                        FertilizeInterval = plant.FertilizeInterval,
                        UserId = userId,
                    };
                    return Ok(plantInfo); 
                }
                return Ok(plant);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
         
        }


        [HttpPost]
        public async Task<ActionResult<Plant>> CreatePlant(int userId, [FromBody] PlantCreation plantObject) // The attribute is optional because the type is complex and will come from body.
        {
            //if (!ModelState.IsValid) // The ApiController attribute takes care of sending this error in case the ModelState is invalid during model binding. 
            //{
            //    return BadRequest();
            //}
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Verify if the plant already exists by checking the name and genus
                if (await _repo.DoesPlantExists(userId, plantObject.Name, plantObject.GenusId)) return Conflict(); 

                // Map our creation model to the real Plants model 
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

                await _repo.CreatePlantAsync(finalPlant);
                await _repo.SaveAllChangesAsync();

                return CreatedAtRoute("GetPlantById", new
                {
                    userId = userId,
                    plantId = finalPlant.Id,
                },
                finalPlant); // Returns a 201 Created status code if successful
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
           
        }
        [HttpPut("{plantId}")]
        public async Task<ActionResult> UpdatePlant(int userId, int plantId, [FromBody] PlantEdit plantObject) // Return type of ActionResult because nothing will be returned. 
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId); ;
                if (!user) return NotFound();

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                plant.Name = plantObject.Name;
                plant.GenusId = plantObject.GenusId;
                plant.DateWatered = plantObject.DateWatered;
                plant.DateFertilized = plantObject.DateFertilized;
                plant.WaterInterval = plantObject.WaterInterval;
                plant.FertilizeInterval = plantObject.FertilizeInterval;

                //var finalPlant = new Plant()
                //{
                //    Name = plantObject.Name,
                //    GenusId = plantObject.GenusId,
                //    DateAdded = DateTime.Now,
                //    DateWatered = plantObject.DateWatered,
                //    DateFertilized = plantObject.DateFertilized,
                //    WaterInterval = plantObject.WaterInterval,
                //    FertilizeInterval = plantObject.FertilizeInterval,
                //    UserId = userId,
                //};

                //plant = finalPlant; 

                await _repo.SaveAllChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
           
        }

        [HttpPatch("{plantId}")]
        public async  Task<ActionResult<Plant>> PatchPlant(int userId, int plantId, [FromBody] JsonPatchDocument<PlantCreation> patchDocument) // Return type of ActionResult because nothing will be returned. 
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Get the original plant to patch
                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                // Map it to a PlantCreation model
                var plantToPatch = new PlantCreation()
                {
                    Name = plant.Name,
                    GenusId = plant.GenusId,
                    DateAdded = plant.DateAdded,
                    DateWatered = plant.DateWatered,
                    DateFertilized = plant.DateFertilized,
                    WaterInterval = plant.WaterInterval,
                    FertilizeInterval = plant.FertilizeInterval,
                };

                patchDocument.ApplyTo(plantToPatch, ModelState);

                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!TryValidateModel(plantToPatch)) return BadRequest(ModelState); // Catches any validation errors applied to the patched object of type PlantCreation. 

                plant.Name = plantToPatch.Name;
                plant.GenusId = plantToPatch.GenusId;
                plant.DateAdded = plantToPatch.DateAdded;
                plant.DateWatered = plantToPatch.DateWatered;
                plant.DateFertilized = plantToPatch.DateFertilized;
                plant.WaterInterval = plantToPatch.WaterInterval;
                plant.FertilizeInterval = plantToPatch.FertilizeInterval;


                await _repo.SaveAllChangesAsync();

                return plant;
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
           

        }

        [HttpDelete("{plantId}")]
        public async Task<ActionResult> DeletePlant(int userId, int plantId)
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId);
                if (!user) return NotFound();

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                await _repo.DeletePlantAsync(userId, plantId);
                await _repo.SaveAllChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
           
        }

        [HttpGet("{plantId}/notes", Name = "GetPlantNotes")]
        public async Task<ActionResult<IEnumerable<PlantNote>>> GetPlantNotes(int userId, int plantId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (IdsDoNotMatch(userId)) return Forbid();

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                // Default pagination
                if (pageSize > maxPageSizeForPlants)
                {
                    pageSize = maxPageSizeForPlants;
                }

                if (!await _userRepo.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var (plantNotes, paginationMetadata) = await _repo.GetPlantNotesAsync(userId, plantId, pageNumber, pageSize);

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Ok(plantNotes);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plant notes for user id {userId} and plant id {plantId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

        [HttpGet("{plantId}/notes/{noteId}", Name = "GetPlantNoteById")]
        public async Task<ActionResult<PlantNote>> GetPlantNoteById(int userId, int plantId, int noteId)
        {
            try
            {
                if (IdsDoNotMatch(userId)) return Forbid();

                if (!await _userRepo.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                var plantNote = await _repo.GetSinglePlantNoteAsync(userId, plantId, noteId);
                if (plantNote == null) return NotFound();

                return Ok(plantNote);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plant note for user id {userId}, plant id {plantId}, and note id {noteId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

        [HttpDelete("{plantId}/notes/{noteId}", Name = "DeletePlantNote")]
        public async Task<ActionResult> DeletePlantNote(int userId, int plantId, int noteId)
        {
            try
            {
                if (IdsDoNotMatch(userId)) return Forbid();

                if (!await _userRepo.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                await _repo.DeletePlantNoteAsync(userId, plantId, noteId);

                await _repo.SaveAllChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plant notes for user id {userId} and plant id {plantId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

        [HttpPost("{plantId}/notes", Name = "CreatePlantNote")]
        public async Task<ActionResult<PlantNote>> CreatePlantNote(int userId, int plantId, [FromBody] PlantNoteCreation plantNoteObject)
        {
            try
            {
                if (IdsDoNotMatch(userId)) return Forbid();

                if (!await _userRepo.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _repo.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                var finalPlantNote = new PlantNote()
                {
                    UserId = userId,
                    PlantId = plantId,
                    Description = plantNoteObject.Description,
                    DateAdded = DateTime.Now,
                    DateEdited = null
                }; 

                await _repo.CreatePlantNoteAsync(finalPlantNote);
                await _repo.SaveAllChangesAsync();

                return CreatedAtRoute("GetPlantNoteById", new
                {
                    userId = userId,
                    plantId = plantId,
                    noteId = finalPlantNote.Id
                },
                finalPlantNote); // Returns a 201 Created status code if successful
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plant notes for user id {userId} and plant id {plantId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

        private Boolean IdsDoNotMatch(int userId)
        {
            var tokenUserId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            return (tokenUserId == null || (tokenUserId != null && userId != Int32.Parse(tokenUserId))); 
        }

        [HttpPatch("{plantId}/notes/{noteId}")]
        public async Task<ActionResult<PlantNote>> PatchPlantNote(int userId, int plantId, int noteId, [FromBody] JsonPatchDocument<PlantNoteCreation> patchDocument) // Return type of ActionResult because nothing will be returned. 
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userRepo.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Get the original plant note to patch
                var plantNote = await _repo.GetSinglePlantNoteAsync(userId, plantId, noteId);
                if (plantNote == null) return NotFound();

                // Map it to a PlantCreation model
                var plantNoteToPatch = new PlantNoteCreation()
                {
                    Description = plantNote.Description
                };

                patchDocument.ApplyTo(plantNoteToPatch, ModelState);

                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!TryValidateModel(plantNoteToPatch)) return BadRequest(ModelState); // Catches any validation errors applied to the patched object of type PlantCreation. 

                plantNote.Description = plantNoteToPatch.Description;
                plantNote.DateEdited = DateAndTime.Now; 


                await _repo.SaveAllChangesAsync();

                return plantNote;
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }


        }
    }
}
