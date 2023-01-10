using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services.JwtFeatures
{
	public class JwtHandler: IJwtHandler
    {
        private readonly IConfiguration _config;

        public JwtHandler(IConfiguration config)
		{
            _config = config;
        }
        public SigningCredentials GetSigningCredentials()
        {
            var pathToSecurityKey = _config["Authentication:SecretForKey"];
            var key = Encoding.UTF8.GetBytes(pathToSecurityKey);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public List<Claim> GetClaims(PlantInfoUser user)
        {
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim(ClaimTypes.Name, user.Id.ToString())); 
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("access_role", user.UserRole.ToString()));
            claimsForToken.Add(new Claim("userId", user.Id.ToString()));
            return claimsForToken;
        }

        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _config["Authentication:Issuer"],
                audience: _config["Authentication:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Authentication:expireInMinutes"])),
                signingCredentials: signingCredentials);
            return tokenOptions;
        }

        // Refresh token generator
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Authentication:SecretForKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if(jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token"); 
            }

            return principal; 
        }

        public string GenerateAccessToken(PlantInfoUser user, List<Claim>? claims)
        {
            var signingCredentials = this.GetSigningCredentials();

            var tokenOptions = this.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token; 
        }

        public string GenerateAccessToken(PlantInfoUser user)
        {
            var signingCredentials = this.GetSigningCredentials();
            var claimsForToken = this.GetClaims(user);
            var tokenOptions = this.GenerateTokenOptions(signingCredentials, claimsForToken);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token;
        }
    }

}

