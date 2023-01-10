namespace Plants.info.API.Models
{
    public class PlantInfoUser
    {
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Email { get; set; }
        public ICollection<Plant> PlantList { get; set; } = new List<Plant>(); // Always initialize collections to avoid null exceptions
        public int UserRole { get; set; } = 8; // Initialize the user role to 8 create an enum for this
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExiryTime { get; set; }
    }
}
