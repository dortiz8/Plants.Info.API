using System;
using System.ComponentModel.DataAnnotations;

namespace Plants.info.API.Data.Models.Authentication
{
	public class GoogleAuthenticateRequestBody
	{
        [Required]
        public string IdToken { get; set; }
        [Required]
        public string Email { get; set; }
    }
}

