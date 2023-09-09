using System;
using System.ComponentModel.DataAnnotations;

namespace Plants.info.API.Data.Models.Authentication
{
	public class AuthenticationRequestBody
	{
        [Required(ErrorMessage = "Username is required.")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }
    }
}

