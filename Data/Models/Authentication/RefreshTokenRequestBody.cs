using System;
namespace Plants.info.API.Data.Models.Authentication
{
	public class RefreshTokenRequestBody
	{
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}

