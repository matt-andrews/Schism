using Microsoft.AspNetCore.Mvc;

namespace Schism.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Get()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}