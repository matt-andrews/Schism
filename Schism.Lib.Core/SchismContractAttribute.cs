using Schism.Lib.Core.Interfaces;

namespace Schism.Lib.Core;

/// <summary>
/// Attribute used for appending metadata to an <see cref="ISchismContract"/> when mapping cannot happen automatically, such as 
/// when the interface is not implemented on the desired controller
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class SchismContractAttribute : Attribute
{
    /// <summary>
    /// The client id that this contract represents. 
    /// <code>Default:</code>
    /// This is defaulted to the interfaces namespace
    /// </summary>
    public string? ClientId { get; set; }
    /// <summary>
    /// The controller/service that this contract represents.
    /// <code>Default:</code>
    /// This is defaulted to the interfaces name
    /// </summary>
    public string? Type { get; set; }
}