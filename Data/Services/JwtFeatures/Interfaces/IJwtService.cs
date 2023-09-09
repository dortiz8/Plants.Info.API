using System;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.JwtFeatures.Interfaces
{
	public interface IJwtService
	{
		Task<Tuple<string, string>> GetAuthenticationTokens(PlantInfoUser user); 
	}
}

