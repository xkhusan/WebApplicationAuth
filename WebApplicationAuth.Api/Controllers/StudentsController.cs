using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplicationAuth.Api.DataBase.Helpers;

namespace WebApplicationAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{UserRoles.Student},{UserRoles.Manager}")]
    public class StudentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() // Student OR Manager.
        {
            return Ok(nameof(StudentsController));
        }
    }
}
