using Example.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Schism.Lib.Core;

namespace Example.Service.Math.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MathController(ISchismClientFactory factory) : ControllerBase, IMathContract
{
    private void ValidateHeader()
    {
        Microsoft.Extensions.Primitives.StringValues header = Request.Headers["x-test-key"];
        if (header.ToString() != "i-am-a-secret-key")
        {
            throw new Exception("Failed to get header a header that we expected to exist");
        }
    }
    [HttpGet]
    public async Task<AdditionResponse> Addition([FromQuery] AdditionRequest req)
    {
        ValidateHeader();
        //convert string args to int's and then return addition
        IConverterContract client = factory.GetClientFor<IConverterContract>();
        StringToIntResponse a = await client.StringToInt(new StringToIntRequest(req.A));
        StringToIntResponse b = await client.StringToInt(new StringToIntRequest(req.B));
        int result = a.Data + b.Data;

        IDataCollectionContract dataCollectionClient = factory.GetClientFor<IDataCollectionContract>();
        await dataCollectionClient.InsertData(new InsertDataRequest($"{req.A} + {req.B}", result.ToString()));

        return new AdditionResponse() { Data = result };
    }

    [HttpGet]
    public async Task<DivisionResponse> Division([FromQuery] DivisionRequest req)
    {
        ValidateHeader();
        int result = req.A / req.B;

        IDataCollectionContract dataCollectionClient = factory.GetClientFor<IDataCollectionContract>();
        await dataCollectionClient.InsertData(new InsertDataRequest($"{req.A} / {req.B}", result.ToString()));

        return new DivisionResponse() { Data = result };
    }

    [HttpGet]
    public async Task<MultiplicationResponse> Multiplication([FromQuery] MultiplicationRequest req)
    {
        ValidateHeader();
        int result = req.A * req.B;

        IDataCollectionContract dataCollectionClient = factory.GetClientFor<IDataCollectionContract>();
        await dataCollectionClient.InsertData(new InsertDataRequest($"{req.A} / {req.B}", result.ToString()));

        return new MultiplicationResponse() { Data = result };
    }

    [HttpGet]
    public async Task<SubtractionResponse> Subtraction([FromQuery] SubtractionRequest req)
    {
        ValidateHeader();
        //convert string args to int's and then return subtraction
        IConverterContract client = factory.GetClientFor<IConverterContract>();
        StringToIntResponse a = await client.StringToInt(new StringToIntRequest(req.A));
        StringToIntResponse b = await client.StringToInt(new StringToIntRequest(req.B));
        int result = a.Data - b.Data;

        IDataCollectionContract dataCollectionClient = factory.GetClientFor<IDataCollectionContract>();
        await dataCollectionClient.InsertData(new InsertDataRequest($"{req.A} - {req.B}", result.ToString()));
        return new SubtractionResponse() { Data = result };
    }
}