using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;

namespace Schism.Tests.Core;
internal class TestSendFeatureProvider
{
    [Test]
    public async Task TestDuplicatesUseLastAdded()
    {
        ISendFeature dupe1 = Substitute.For<ISendFeature>();
        dupe1.Key.Returns("my-key");
        ISendFeature dupe2 = Substitute.For<ISendFeature>();
        dupe2.Key.Returns("my-key");
        dupe2.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>())
            .Returns(Task.FromResult(new SchismResponse(null!) { StatusCode = System.Net.HttpStatusCode.OK }));

        SendFeatureProvider.SendFeatureCollection builder = new();
        builder.AddFeature(_ => dupe1);
        builder.AddFeature(_ => dupe2);

        ServiceProvider serviceProvider = new ServiceCollection().BuildServiceProvider();

        ISendFeatureProvider result = builder.Build(serviceProvider);
        Assert.That(result.Count(), Is.EqualTo(1));

        ISendFeature feat = result.First(f => f.Key == "my-key");
        await feat.SendAsync(new SchismRequest("clientid", [], "baseuri", null!), new ConnectionPoint()
        {
            Name = "name",
            Path = "path",
            Type = "type"
        });
        _ = dupe1.Received(0).SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>());
        _ = dupe2.Received(1).SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>());
    }
}