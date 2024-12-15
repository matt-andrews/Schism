using Microsoft.AspNetCore.Mvc;
using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Abstractions.Models;

namespace Schism.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class RefreshController(IRefreshService refreshService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(RefreshRequest request)
    {
        return Ok(await refreshService.Refresh(request));
    }
}