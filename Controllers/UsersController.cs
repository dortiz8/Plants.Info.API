using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.AspNetCore.Cors;

namespace Plants.info.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [EnableCors("CorsPolicy")]
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
        [HttpGet("{userId}")]
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
                    UserName = user.UserName
                });   // Will return the city with 200 status code
            }
            catch (Exception ex)
            {
                _log.LogCritical($"Exception while getting user with Id: {userId}", ex);
                return StatusCode(500, "A problem occurred while handling your request");
            }
      
        }
      
    }
}
