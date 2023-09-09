using System;
using Plants.info.API.Data.Models.Authentication;
using Plants.info.API.Data.Repository;
using Plants.info.API.Data.Services.JwtFeatures.Interfaces;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.JwtFeatures
{
	public class JwtService : IJwtService
	{
        public readonly IJwtHandler _jwtHandler; 
        private readonly IUserRepository _userRepo;

        public JwtService(IJwtHandler jwtHandler, IUserRepository userRepository)
		{
            _jwtHandler = jwtHandler;
            _userRepo = userRepository; 
		}

        public async Task<Tuple<string, string>> GetAuthenticationTokens(PlantInfoUser user)
        {
            var token = _jwtHandler.GenerateAccessToken(user);

            var refreshToken = _jwtHandler.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExiryTime = DateTime.Now.AddDays(2);

            await _userRepo.SaveAllChangesAsync();

            return new Tuple<string, string>( token, refreshToken );
        }
    }
}

