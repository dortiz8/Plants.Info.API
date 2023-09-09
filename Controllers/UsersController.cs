using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace Plants.info.API.Controllers
{
    [Route("api/users")]
    [EnableCors("CorsPolicy")]
   
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IPlantsRepository _plantRepo;
        private readonly ILogger<PlantsController> _log;
        public UsersController(IUserRepository repo, IPlantsRepository plantRepo, ILogger<PlantsController> logger)
        {
            _repo = repo;
            _plantRepo = plantRepo;
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserOnly>>> GetAllUsers()
        {
            try
            {
                var users = await _repo.GetAllUsersAsync();
                var usersOnly = new List<UserOnly>(); 
                if( users.Any())
                {
                    foreach (var user in users)
                    {
                        usersOnly.Add(new UserOnly()
                        {
                            UserName = user.UserName,
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName
                        }); 
                    }
                }
                return Ok(usersOnly);    // Will return the city with 200 status code);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting all users", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
            
        }
        [HttpGet("{userId}", Name = "getUser")]
        public async Task<IActionResult> GetUserById(int userId, bool includePlants = false) // Return a generic IActionResult to account for returning two different types of classes.
        {
            try
            {
                var user = await _repo.GetUserByIdAsync(userId);
                if (user == null) return NotFound(); // Will return 404 status code
                if (includePlants)
                {
                    user.PlantList = (ICollection<Plant>)await _plantRepo.GetPlantsByIdAsync(userId); 
                    return Ok(user); 
                }
                return Ok(new UserOnly
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName
              
                });   // Will return the city with 200 status code
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting user with Id: {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
      
        }
        //[Authorize(Policy = "RoleMustBeAdmin")] // Makes sure we are authenticated with a token before accessing this endpoint
        [HttpPost]
        public async Task<ActionResult<PlantInfoUser>> CreateUser( UserCreation userCreationObject)
        {
            try
            {
                

                // Verify if username exists
                if (await _repo.UserNameExistsAsync(userCreationObject.UserName)) return Conflict(); 

                
                var newUser = new PlantInfoUser()
                {
                    FirstName = userCreationObject.Name,
                    LastName = userCreationObject.Lname,
                    UserName = userCreationObject.UserName,
                    Password = userCreationObject.Password,
                    Email = userCreationObject.Email,
                    CreatedDate = DateTime.Now,
                };



                await _repo.CreateUserAsync(newUser);
                await _repo.SaveAllChangesAsync();

                var userOnly = new UserOnly()
                {
                    Id = newUser.Id,
                    UserName = userCreationObject.UserName,
                    FirstName = userCreationObject.Name,
                    LastName = userCreationObject.Lname
                }; 

                return CreatedAtRoute("getUser", new
                { userId = userOnly.Id },
                userOnly); 
                   

            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while creating new user.", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
        }

        [Authorize(Policy = "RoleMustBeAdmin")] // Makes sure we are authenticated with a token before accessing this endpoint
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            try
            {
                var user = await _repo.GetUserByIdAsync(userId);
                if (user == null) return NotFound(); // Will return 404 status code

                await _repo.DeleteUserAsync(userId);
                await _repo.SaveAllChangesAsync();

                return NoContent(); 

            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while deleting user with ID: {userId}.", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }


        }

        private Boolean IdsDoNotMatch(int userId)
        {
            var tokenUserId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            return (tokenUserId == null || (tokenUserId != null && userId != Int32.Parse(tokenUserId)));
        }

    }
}
