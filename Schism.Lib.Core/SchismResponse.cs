using Schism.Lib.Core.Interfaces;
using System.Net;

namespace Schism.Lib.Core;
public record SchismResponse(ISchismSerializer Serializer)
{
    public required HttpStatusCode StatusCode { get; set; }
    public byte[]? Content { get; set; }

    public async Task<string> ContentAsStringAsync()
    {
        if (Content is null)
        {
            return string.Empty;
        }

        StreamReader reader = new(new MemoryStream(Content));
        return await reader.ReadToEndAsync();
    }
    public async Task<T?> ContentAsJsonAsync<T>()
    {
        return Content is null ? default : await Serializer.DeserializeAsync<T>(new MemoryStream(Content));
    }
}