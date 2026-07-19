using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Diagnostics;
using WebApplication.DataBase;
using WebApplication.DataBase.Models;
using WebApplication.ViewModels;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        // UserManager is C# class from Microsoft.AspNetCore.Identity which provides the APIs for managing user in a persistence store (in our case MS SQL Server DB).
        private readonly UserManager<ApplicationUser> _userManager;

        // RoleManager provides the APIs for managing roles in a persistence store (in our case we are not using custom role like ApplicationUser derived from IdentityUser but Identity Role).
        private readonly RoleManager<IdentityRole> _roleManager;

        // A DbContext instance represents a session with the database and can be used to query and save instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
        private AppDbContext _context;

        // To get some data from appsettings.json.
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserVM registerUserVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _userManager.FindByEmailAsync(registerUserVM.EmailAddress) is not null)
            {
                return BadRequest($"User {registerUserVM.EmailAddress} already exists.");
            }

            if ((await _userManager.CreateAsync(new ApplicationUser()
            {
                FirstName = registerUserVM.FirstName,
                LastName = registerUserVM.LastName,
                Email = registerUserVM.EmailAddress,
                UserName = registerUserVM.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
            }, registerUserVM.PassWord)).Succeeded)
            {
                return Ok("User created.");
            }

            return BadRequest("User could not be created.");
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserVM loginUserVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(loginUserVM.EmailAddress);

            if (userExists != null && await _userManager.CheckPasswordAsync(userExists, loginUserVM.PassWord))
            {
                return Ok("User signed in.");
            }

            return Unauthorized();
        }
    }
}
