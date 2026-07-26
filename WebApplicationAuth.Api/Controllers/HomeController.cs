using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Specifies that the class or method that this attribute is applied to requires the specified authorization.
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
