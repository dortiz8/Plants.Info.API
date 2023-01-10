using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Models;
using Plants.info.API.Models;
using System.Text.Json;

namespace Plants.info.API.Data
{
    public class UserSeeder
    {
        private readonly UserContext _ctx;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public UserSeeder(UserContext ctx, IConfiguration config, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _config = config;
            _env = env;
        }


        // Create Order
        public void SeedSampleData()
        {
            _ctx.Database.EnsureCreated(); // Make sure db exists

            // Only seed sample data if there are no plants
            if (!_ctx.Plants.Any())
            {
                var path = Path.Combine(_env.ContentRootPath, _config["Paths:JsonSamplePlants"]);
                //Create sample data 
                var json = File.ReadAllText(path);
                // Deserialize the json object into an enumerable of Plants
                var samplePlants = JsonSerializer.Deserialize<IEnumerable<Plant>>(json);

                if (samplePlants.Any())
                {
                    _ctx.Plants.AddRange(samplePlants);
                }

                var sampleUser = new PlantInfoUser();
                
                sampleUser.UserName = "dortiz8";
                sampleUser.Password = "RandomPassWord12";
                sampleUser.Email = "dort08@gmail.com";
                sampleUser.CreatedDate = DateTime.Now; 
                sampleUser.PlantList = new List<Plant>((IEnumerable<Plant>)samplePlants);

                Console.WriteLine(samplePlants); 
                _ctx.Users.Add(sampleUser);
                _ctx.SaveChanges(); 
            }
        }
    }
}
