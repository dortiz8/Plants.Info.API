using Plants.info.API.Models;

namespace Plants.info.API.Data.Models
{
    public class UserCreation
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Email { get; set; }
        public ICollection<Plant> PlantList { get; set; } = new List<Plant>();
        public int UserRole { get; set; } = 8; 
    }
}
