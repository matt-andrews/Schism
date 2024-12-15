using Example.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Example.Service.Converter.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ConverterController : ControllerBase, IConverterContract
{
    [HttpGet]
    public Task<StringToIntResponse> StringToInt([FromQuery] StringToIntRequest req)
    {
        _ = int.TryParse(req.Data, out int result);
        return Task.FromResult(new StringToIntResponse() { Data = result });
    }
}