using Microsoft.AspNetCore.Mvc;
using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Abstractions.Models;

namespace Schism.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController(IRegisterService registerService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(RegistrationRequest request)
    {
        return Ok(await registerService.Register(request));
    }
}