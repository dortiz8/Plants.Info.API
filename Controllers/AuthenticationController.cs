using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Plants.info.API.Data.Models;
using Plants.info.API.Data.Repository;
using Plants.info.API.Data.Services.JwtFeatures;
using Plants.info.API.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Plants.info.API.Controllers
{
    [Route("api/authentication")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepo;
        public IJwtHandler _jwtHandler { get; }

        public AuthenticationController(IConfiguration configuration, IUserRepository userRepository, IJwtHandler jwtHandler)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userRepo = userRepository;
            _jwtHandler = jwtHandler;
        }
        // Request body expected when a user sends a request to the above endpoint
        public class AuthenticationRequestBody
        {
            [Required(ErrorMessage = "Username is required.")]
            public string? UserName { get; set; }
            [Required(ErrorMessage = "Password is required.")]
            public string? Password { get; set; }
        }

        // Authentication Response
        public class AuthResponseBody
        {
            public bool IsAuthSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
            public string? Token { get; set; }
            public string? RefreshToken { get; set; }
        }

        public class RefreshTokenRequestBody
        {
            public string? Token { get; set; }
            public string? RefreshToken { get; set; }
        }


        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate([FromBody] AuthenticationRequestBody authenticationRequestBody)
        {
            // Step #1 Validate user credentials
            var user = await _userRepo.FindUserByUsernameAsync(authenticationRequestBody.UserName);
            if(user == null || ValidateUserCreds(user, authenticationRequestBody.Password))
            {
                return Unauthorized(new AuthResponseBody { ErrorMessage = "Invalid Authentication" }); 
            }

            // Step #2 & #3 create a security key and signing credentials

         
            var token = _jwtHandler.GenerateAccessToken(user); 

            var refreshToken = _jwtHandler.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExiryTime = DateTime.Now.AddDays(7);

            await _userRepo.SaveAllChangesAsync(); 


            // Return a response body with the token included
            return Ok(new AuthResponseBody { IsAuthSuccessful = true, Token = token, RefreshToken = refreshToken }); 

        }

        private bool ValidateUserCreds(PlantInfoUser user, string? password) => (user.Password == password); 
     
    }
}

