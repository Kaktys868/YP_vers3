using Microsoft.AspNetCore.Mvc;

namespace Bank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API работает!", timestamp = DateTime.Now });
        }

        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok(new { message = "Привет от API!" });
        }

        [HttpPost("login")]
        public IActionResult SimpleLogin()
        {
            return Ok(new
            {
                token = "test-jwt-token-12345",
                username = "admin",
                message = "Логин успешен!"
            });
        }
    }
}
