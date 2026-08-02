using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplicationAuth.Api.DataBase;
using WebApplicationAuth.Api.DataBase.Models;
using WebApplicationAuth.Api.ViewModels;

namespace WebApplicationAuth.Api.Services
{
    public class AuthorizationService(
        IConfiguration configuration,
        AppDbContext dbContext,
        TokenValidationParameters tokenValidationParameters,
        ILogger<AuthorizationService> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // To get key value pairs from appsettings.json.
        private readonly IConfiguration _configuration = configuration;
        // A DbContext instance represents a session with the database and can be used to query and save instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
        private readonly AppDbContext _dbContext = dbContext;
        private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;
        private readonly ILogger<AuthorizationService> _logger = logger;
        // UserManager is C# class from Microsoft.AspNetCore.Identity which provides the APIs for managing user in a persistence store (in our case MS SQL Server DB).
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        // RoleManager provides the APIs for managing roles in a persistence store (in our case we are not using custom role like ApplicationUser derived from IdentityUser but Identity Role).
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<IActionResult> RegisterUserAsync(RegisterUserVM registerUserVM)
        {
            if (registerUserVM is null)
                return new BadRequestObjectResult("Invalid request.");

            if (await _userManager.FindByEmailAsync(registerUserVM.EmailAddress) is not null)
                return new BadRequestObjectResult($"User {registerUserVM.EmailAddress} already exists.");

            var user = new ApplicationUser()
            {
                FirstName = registerUserVM.FirstName,
                LastName = registerUserVM.LastName,
                Email = registerUserVM.EmailAddress,
                UserName = registerUserVM.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerUserVM.PassWord);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(registerUserVM.UserRole) && await _roleManager.RoleExistsAsync(registerUserVM.UserRole))
                {
                    await _userManager.AddToRoleAsync(user, registerUserVM.UserRole);
                }

                return new OkObjectResult("User created.");
            }

            return new BadRequestObjectResult(result.Errors);
        }

        public async Task<IActionResult> LoginUserAsync(LoginUserVM loginUserVM)
        {
            if (loginUserVM is null)
                return new BadRequestObjectResult("Invalid request.");

            var user = await _userManager.FindByEmailAsync(loginUserVM.EmailAddress);

            if (user is not null && await _userManager.CheckPasswordAsync(user, loginUserVM.PassWord))
            {
                var token = await GenerateJWTTokenAsync(user, null);
                return new OkObjectResult(token);
            }

            return new UnauthorizedResult();
        }

        private async Task<AuthResultVM> GenerateJWTTokenAsync(ApplicationUser user, RefreshToken? token)
        {
            ArgumentNullException.ThrowIfNull(user);

            var accessToken = await CreateAccessTokenAsync(user);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(accessToken);

            if (token is not null)
            {
                token.IsRevoked = true;
            }

            var refreshToken = CreateRefreshToken(user, accessToken);
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResultVM()
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token!,
                ExpiresAt = accessToken.ValidTo,
            };
        }

        public async Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVM tokenRequestVM)
        {
            ArgumentNullException.ThrowIfNull(tokenRequestVM);

            var refreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(t => t.Token == tokenRequestVM.RefreshToken);

            if (refreshToken?.User is null)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                throw new SecurityTokenException("Refresh token has been revoked.");
            }

            if (refreshToken.ExpiresAt < DateTimeOffset.UtcNow)
            {
                throw new SecurityTokenException("Refresh token has expired.");
            }

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var accessTokenValidationParameters = _tokenValidationParameters.Clone();
            accessTokenValidationParameters.ValidateLifetime = false;

            try
            {
                var principal = jwtSecurityTokenHandler.ValidateToken(tokenRequestVM.AccessToken, accessTokenValidationParameters, out _);
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.Equals(jti, refreshToken.JwtId, StringComparison.Ordinal))
                {
                    throw new SecurityTokenException("Token mismatch.");
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Invalid access token supplied during refresh flow.");
                throw;
            }

            return await GenerateJWTTokenAsync(refreshToken.User, refreshToken);
        }

        private async Task<JwtSecurityToken> CreateAccessTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!));

            return new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTimeOffset.UtcNow.AddMinutes(8).UtcDateTime,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        }

        private RefreshToken CreateRefreshToken(ApplicationUser user, JwtSecurityToken accessToken)
        {
            return new RefreshToken()
            {
                JwtId = accessToken.Id,
                IsRevoked = false,
                UserId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMonths(8),
                Token = $"{Guid.NewGuid()}-{Guid.NewGuid()}",
            };
        }
    }
}
