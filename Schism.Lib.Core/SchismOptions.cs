using Microsoft.Extensions.Configuration;

namespace Schism.Lib.Core;

public sealed record SchismOptions
{
    /// <summary>
    /// The immutable identifier for this instance
    /// </summary>
    public Guid InstanceId { get; } = Guid.NewGuid();
    /// <summary>
    /// Determines the number of seconds in between each Host registration request and Client refresh requests
    /// <code>Default:</code>
    /// 300 seconds
    /// </summary>
    public int Refresh { get; set; } = 300;
    /// <summary>
    /// The host for connecting to this application
    /// <code>Required</code>
    /// </summary>
    public string Host { get; set; } = default!;
    /// <summary>
    /// The Client Id for pointing to this application
    /// <code>Default:</code>
    /// This defaults to the application assembly name
    /// </summary>
    public string ClientId { get; set; } = default!;
    /// <summary>
    /// The Hub URI for service discovery
    /// <code>Required</code>
    /// </summary>
    public string HubUri { get; set; } = default!;
    /// <summary>
    /// The namespace of the assembly. This gets overwritten by the executing assembly name
    /// </summary>
    public string Namespace { get; set; } = "";

    internal SchismOptions() { }
    /// <summary>
    /// Binds the section `Schism` to this object
    /// </summary>
    /// <param name="config"></param>
    public SchismOptions(IConfiguration config)
    {
        IConfigurationSection section = config.GetSection("Schism");
        section.Bind(this);
        ArgumentNullException.ThrowIfNull(nameof(HubUri));
        ArgumentNullException.ThrowIfNull(nameof(Host));
    }
}