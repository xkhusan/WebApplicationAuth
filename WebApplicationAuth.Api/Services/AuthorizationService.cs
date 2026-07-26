using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplicationAuth.Api.DataBase.Models;
using WebApplicationAuth.Api.ViewModels;

namespace WebApplicationAuth.Api.Services
{
    public class AuthorizationService
    {
        private readonly IConfiguration _configuration;
        public AuthorizationService(IConfiguration configuration) => _configuration = configuration;

        public async Task<AuthResultVM> GenerateJWTTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Sub, user?.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTimeOffset.UtcNow.AddMinutes(1).UtcDateTime,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var authResult = new AuthResultVM()
            {
                Token = jwtToken,
                ExpiresAt = token.ValidTo,
            };

            return authResult;
        }
    }
}
