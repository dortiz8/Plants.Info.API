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
using Plants.info.API.Data.Services.PlantServices.Interfaces;
using Plants.info.API.Data.Services.UserServices;
using Plants.info.API.Data.Services.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Plants.info.API.Controllers
{
   [Authorize] //we have no way to pass down id values to this policy, therefore a custom attribute needs to be created but it is not recommended.
    [Route("api/users/{userId}/plants")] // Since we need to gather user information to get plants we reflect the URL as such and set plants as a child resource of Users. 
    [ApiController]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantsService _plantsService;
        private readonly IUserService _userService;
        private readonly IPlantService _plantService;
        private readonly ILogger<PlantsController> _log;
        const int maxPageSizeForPlants = 100;
        
        public PlantsController(IPlantsService plantsService, IUserService userService, IPlantService plantService,
             ILogger<PlantsController> logger)
        {
            _plantsService = plantsService;
            _userService = userService;
            _plantService = plantService;
            _log = logger ?? throw new ArgumentNullException(nameof(logger)); // Null check in case the container changes and it returns a null value
        }


        [HttpGet] // This is sufficient since we defined the templete at the controller level. 
        public async Task<ActionResult<IEnumerable<Plant>>> GetPlantsByUserId(int userId, [FromQuery] string? name, 
                                                                                [FromQuery] string? queryString, [FromQuery] int pageNumber = 1, 
                                                                                [FromQuery] int pageSize = 50)
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if(IdsDoNotMatch(userId)) return Forbid(); // returns 403 code


                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                // Default pagination
                if (pageSize > maxPageSizeForPlants)
                {
                    pageSize = maxPageSizeForPlants; 
                }

                var (plants, paginationMetadata) = await _plantsService.GetPlantsById(userId, name, queryString, pageNumber, pageSize);

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
        public async Task<ActionResult<Plant>> GetPlantByUserId(int userId, int plantId, [FromQuery] bool info = false) // Make sure parameter names match when dealing with multiple parameters. 
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code

                var user = await _userService.UserExistsAsync(userId);
                if (!user) return NotFound();

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                if (info)
                {
                    var plantInfo = await _plantService.GetSinglePlantByIdWithInfoAsync(userId, plant); 
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

                var user = await _userService.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Verify if the plant already exists by checking the name and genus
                if (await _plantService.DoesPlantExists(userId, plantObject.Name, plantObject.GenusId)) return Conflict();

                var finalPlant = await _plantService.CreatePlantAsync(userId, plantObject); 

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

                var user = await _userService.UserExistsAsync(userId); ;
                if (!user) return NotFound();

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                await _plantService.UpdatePlantAsync(userId, plant, plantObject); 

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

                var user = await _userService.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Get the original plant to patch
                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                // ***** TO DO Implement image patching 
                //patchDocument.Operations.ForEach(o =>
                //{
                //    if (o.path == "/image") Console.WriteLine(o.path);
                //}); 
                //var plantToPatch = await _plantService.MapPlantToPatch(userId, plant);
                var plantToPatch = MapService.MapToPlantCreation(plant); 

                patchDocument.ApplyTo(plantToPatch, ModelState);

                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!TryValidateModel(plantToPatch)) return BadRequest(ModelState); // Catches any validation errors applied to the patched object of type PlantCreation. 

                var plantInfo = await _plantService.PatchPlantAsync(userId, plant, plantToPatch);

                return plantInfo;
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

                var user = await _userService.UserExistsAsync(userId);
                if (!user) return NotFound();

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                await _plantService.DeletePlantAsync(userId, plantId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while deleting plant {plantId} for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
           
        }

        [HttpGet("{plantId}/notes", Name = "GetPlantNotes")]
        public async Task<ActionResult<IEnumerable<PlantNote>>> GetPlantNotes(int userId, int plantId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (IdsDoNotMatch(userId)) return Forbid();

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                // Default pagination
                if (pageSize > maxPageSizeForPlants)
                {
                    pageSize = maxPageSizeForPlants;
                }

                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var (plantNotes, paginationMetadata) = await _plantService.GetPlantNotesAsync(userId, plantId, pageNumber, pageSize);

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

                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                var plantNote = await _plantService.GetSinglePlantNoteAsync(userId, plantId, noteId);
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

                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();

                await _plantService.DeletePlantNoteAsync(userId, plantId, noteId);

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

                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var plant = await _plantService.GetSinglePlantByIdAsync(userId, plantId);
                if (plant == null) return NotFound();


                var finalPlantNote = await _plantService.CreatePlantNoteAsync(userId, plantId, plantNoteObject); 

                return CreatedAtRoute("GetPlantNoteById", new
                {
                    userId = userId,
                    plantId = plantId,
                    noteId = finalPlantNote?.Id
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

                var user = await _userService.UserExistsAsync(userId);
                if (!user) return NotFound();

                // Get the original plant note to patch
                var plantNote = await _plantService.GetSinglePlantNoteAsync(userId, plantId, noteId);
                if (plantNote == null) return NotFound();

                // Map it to a PlantCreation model
                var plantNoteToPatch = MapService.MapToPlantNoteCreation(plantNote); 

                patchDocument.ApplyTo(plantNoteToPatch, ModelState);

                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (!TryValidateModel(plantNoteToPatch)) return BadRequest(ModelState); // Catches any validation errors applied to the patched object of type PlantCreation. 

                await _plantService.PatchPlantNoteAsync(plantNote, plantNoteToPatch); 

                return plantNote;
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }


        }

        [HttpGet("stats")] 
        public async Task<ActionResult<PlantsStats>> GetPlantsStatsByIdAsync(int userId)
        {
            try
            {
                // Verify user Id matches with the user Id specified in the token
                if (IdsDoNotMatch(userId)) return Forbid(); // returns 403 code


                if (!await _userService.UserExistsAsync(userId))
                {
                    _log.LogInformation($"The user with id {userId} was not found.");
                    return NotFound();
                }

                var stats = await _plantsService.GetPlantsStatsByIdAsync(userId);
               
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting plants stats for user id {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }
    }
}
