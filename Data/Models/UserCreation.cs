using Plants.info.API.Models;

namespace Plants.info.API.Data.Models
{
    public class UserCreation
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? Lname { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Email { get; set; }
    }
}
