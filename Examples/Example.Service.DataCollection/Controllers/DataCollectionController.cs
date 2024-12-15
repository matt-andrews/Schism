using Example.Service.DataCollection.Services;
using Example.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Schism.Lib.ServiceBus;

namespace Example.Service.DataCollection.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DataCollectionController(DataCollectionStore store) : ControllerBase, IDataCollectionContract
{
    private void ValidateHeader()
    {
        Microsoft.Extensions.Primitives.StringValues header = Request.Headers["x-test-key"];
        if (header.ToString() == "i-am-a-secret-key")
        {
            throw new Exception("Header validation failed, header exists that we didn't want to exist");
        }
    }
    [HttpGet]
    public Task<GetDataResponse> GetData()
    {
        ValidateHeader();
        string[] data = store.Get().Select(s => $"{s.Request} = {s.Result}").ToArray();
        return Task.FromResult(new GetDataResponse() { Data = data });
    }

    [SchismServiceBusQueue(Queue = "data-collection-queue")]
    [HttpPost]
    public Task InsertData(InsertDataRequest req)
    {
        store.Add(req);
        return Task.CompletedTask;
    }

    [HttpDelete]
    public Task ResetStore()
    {
        ValidateHeader();
        store.Clear();
        return Task.CompletedTask;
    }
}