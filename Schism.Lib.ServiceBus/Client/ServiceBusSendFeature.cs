using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.ServiceBus.Host;

namespace Schism.Lib.ServiceBus.Client;
internal class ServiceBusSendFeature(ISchismServiceBusFactory factory) : ISendFeature
{
    public string Key { get; } = SbConsts.ServiceBusKey;

    public async Task<SchismResponse> SendAsync(SchismRequest request, ConnectionPoint point)
    {
        ISchismServiceBusClient client = await factory.GetClient(request.ClientId, point.Path);
        if (!point.Props.TryGetValue(SbConsts.ServiceBusTopicOrQueueNameKey, out string? topic))
        {
            throw new ArgumentException("Topic or Queue name is required");
        }
        object jsonBody = request.GetProp(SchismRequest.ConstBody)
            ?? throw new ArgumentException("JsonBody is required");
        await client.SendMessage(topic, jsonBody);
        return new SchismResponse(request.Serializer) { StatusCode = System.Net.HttpStatusCode.OK };
    }
}