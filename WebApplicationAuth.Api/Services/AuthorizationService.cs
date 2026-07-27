using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplicationAuth.Api.DataBase;
using WebApplicationAuth.Api.DataBase.Models;
using WebApplicationAuth.Api.ViewModels;

namespace WebApplicationAuth.Api.Services
{
    public class AuthorizationService(IConfiguration configuration, AppDbContext dbContext, TokenValidationParameters tokenValidationParameters)
    {
        private readonly IConfiguration _configuration = configuration;
        // A DbContext instance represents a session with the database and can be used to query and save instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
        private readonly AppDbContext _dbContext = dbContext;
        private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;

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
                expires: DateTimeOffset.UtcNow.AddMinutes(8).UtcDateTime,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user?.Id!,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMonths(8),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            var authResult = new AuthResultVM()
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo,
            };

            return authResult;
        }
    }
}
