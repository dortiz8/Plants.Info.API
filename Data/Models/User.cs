namespace Plants.info.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public ICollection<Plant> PlantList {get; set; } = new List<Plant>(); // Always initialize collections to avoid null exceptions
    }
}
