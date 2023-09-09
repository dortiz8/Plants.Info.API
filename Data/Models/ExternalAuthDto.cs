using System;
namespace Plants.info.API.Data.Models
{
	public class ExternalAuthDto
	{
        public string? Provider { get; set; }
        public string? IdToken { get; set; }
    }
}

