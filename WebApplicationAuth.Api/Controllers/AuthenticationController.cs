using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationAuth.Api.DataBase.Models;
using WebApplicationAuth.Api.Services;
using WebApplicationAuth.Api.ViewModels;

namespace WebApplicationAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        // UserManager is C# class from Microsoft.AspNetCore.Identity which provides the APIs for managing user in a persistence store (in our case MS SQL Server DB).
        private readonly UserManager<ApplicationUser> _userManager;

        // RoleManager provides the APIs for managing roles in a persistence store (in our case we are not using custom role like ApplicationUser derived from IdentityUser but Identity Role).
        private readonly RoleManager<IdentityRole> _roleManager;

        // To get some data from appsettings.json.
        private readonly IConfiguration _configuration;

        private readonly AuthorizationService _authorizationService;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            AuthorizationService authorizationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _authorizationService = authorizationService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserVM registerUserVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userManager.FindByEmailAsync(registerUserVM.EmailAddress) is not null)
                return BadRequest($"User {registerUserVM.EmailAddress} already exists.");

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
                return Ok("User created.");

            return BadRequest(result.Errors);
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserVM loginUserVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var user = await _userManager.FindByEmailAsync(loginUserVM.EmailAddress);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginUserVM.PassWord))
            {
                var token = await _authorizationService.GenerateJWTTokenAsync(user, null);
                return Ok(token);
            }

            return Unauthorized();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenRequestVM tokenRequestVM)
        {
            var result = await _authorizationService.VerifyAndGenerateTokenAsync(tokenRequestVM);
            return Ok(result);
        }
    }
}
