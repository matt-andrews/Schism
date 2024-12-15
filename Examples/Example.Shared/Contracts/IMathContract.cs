using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using System.Text.Json.Serialization;

namespace Example.Shared.Contracts;

[SchismContract(ClientId = "Example.Service.Math")]
public interface IMathContract : ISchismContract
{
    Task<AdditionResponse> Addition(AdditionRequest req);
    Task<SubtractionResponse> Subtraction(SubtractionRequest req);
    Task<DivisionResponse> Division(DivisionRequest req);
    Task<MultiplicationResponse> Multiplication(MultiplicationRequest req);
}
public record AdditionRequest(string A, string B);
public record AdditionResponse
{
    [JsonPropertyName("data")]
    public int Data { get; set; }
}
public record SubtractionRequest(string A, string B);
public record SubtractionResponse
{
    [JsonPropertyName("data")]
    public int Data { get; set; }
}
public record DivisionRequest(int A, int B);
public record DivisionResponse
{
    [JsonPropertyName("data")]
    public int Data { get; set; }
}
public record MultiplicationRequest(int A, int B);
public record MultiplicationResponse
{
    [JsonPropertyName("data")]
    public int Data { get; set; }
}