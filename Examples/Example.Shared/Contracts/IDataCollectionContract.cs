using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using System.Text.Json.Serialization;

namespace Example.Shared.Contracts;
[SchismContract(ClientId = "Example.Service.DataCollection")]
public interface IDataCollectionContract : ISchismContract
{
    Task InsertData(InsertDataRequest req);
    Task<GetDataResponse> GetData();
    Task ResetStore();
}

public record InsertDataRequest(string Request, string Result);

public record GetDataResponse
{
    [JsonPropertyName("data")]
    public string[] Data { get; set; } = [];
}