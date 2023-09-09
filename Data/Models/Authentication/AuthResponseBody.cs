using System;
namespace Plants.info.API.Data.Models.Authentication
{
	public class AuthResponseBody
	{
        public bool IsAuthSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public int? UserId { get; set; }
    }
}

