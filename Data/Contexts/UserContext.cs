using Plants.info.API.Models;
using Microsoft.EntityFrameworkCore;
using Plants.info.API.Data.Models;

namespace Plants.info.API.Data.Contexts
{
    public class UserContext: DbContext
    {
        private readonly IConfiguration _config;

        public UserContext(IConfiguration config)
        {
            _config = config;
        }


        public DbSet<PlantInfoUser> Users { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Genus> Genus { get; set; }
        public DbSet<PlantNote> PlantNotes { get; set; }

        // Configure the context to a specific database = UserDb
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            //base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer(_config["ConnectionStrings:UserContextDb"]);

            var connectionString = configuration.GetConnectionString("UserContextDb"); 
            optionsBuilder.UseSqlServer(connectionString);  
        }
        // Specify how the mapping is going to happen between your entities and the db
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
