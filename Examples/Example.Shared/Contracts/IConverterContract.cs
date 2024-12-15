using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using System.Text.Json.Serialization;

namespace Example.Shared.Contracts;

[SchismContract(ClientId = "Example.Service.Converter")]
public interface IConverterContract : ISchismContract
{
    Task<StringToIntResponse> StringToInt(StringToIntRequest req);
}
public record StringToIntRequest(string Data);
public record StringToIntResponse
{
    [JsonPropertyName("data")]
    public int Data { get; set; }
}