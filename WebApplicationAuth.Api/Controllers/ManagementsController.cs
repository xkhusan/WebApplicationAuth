using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationAuth.Api.DataBase.Helpers;

namespace WebApplicationAuth.Api.Controllers
{
    [Authorize(Roles = UserRoles.Manager)]
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementsController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> GetAsync() // Manager AND Administrator.
        {
            return new OkResult();
        }

        [HttpDelete("manager")]
        public IActionResult RemoveManager() // Manager.
        {
            return Ok("Manager removed.");
        }
    }
}
