using Microsoft.AspNetCore.Mvc;
using WebApplicationAuth.Api.Services;
using WebApplicationAuth.Api.ViewModels;

namespace WebApplicationAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthorizationService _authorizationService;

        public AuthenticationController(AuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserVM registerUserVM)
        {
            return await _authorizationService.RegisterUserAsync(registerUserVM);
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginUserVM loginUserVM)
        {
            return await _authorizationService.LoginUserAsync(loginUserVM);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenRequestVM tokenRequestVM)
        {
            var result = await _authorizationService.VerifyAndGenerateTokenAsync(tokenRequestVM);
            return Ok(result);
        }
    }
}
