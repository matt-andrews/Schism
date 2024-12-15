using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;

namespace Schism.Tests.Core;

internal class TestSchismHost
{
    [Test]
    public async Task TestRefreshInFuture()
    {
        ISchismHubClient http = Substitute.For<ISchismHubClient>();
        SchismHost host = new(TestHelpers.BuildOptions(), [], "0.0.0", TestHelpers.MockLogger<SchismHost>(), http);
        http.PostRegistrationRequest(Arg.Any<RegistrationRequest>()).Returns(Task.FromResult(new RegistrationResponse()));
        Assert.That(await host.Invoke(), Is.True);
        Assert.That(await host.Invoke(), Is.False);
    }
    [Test]
    public async Task TestRefreshInPast()
    {
        ISchismHubClient http = Substitute.For<ISchismHubClient>();
        //Set the refresh config to a negative number to artificially force refresh timer to be in the past
        SchismHost host = new(TestHelpers.BuildOptions(new Dictionary<string, string?>() { { "Schism:Refresh", "-60" } }), [], "0.0.0", TestHelpers.MockLogger<SchismHost>(), http);
        http.PostRegistrationRequest(Arg.Any<RegistrationRequest>()).Returns(Task.FromResult(new RegistrationResponse()));
        Assert.That(await host.Invoke(), Is.True);
        Assert.That(await host.Invoke(), Is.True);
    }
}