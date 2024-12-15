using System.Diagnostics.CodeAnalysis;

namespace Schism.Lib.Core;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public abstract class SchismAttribute : Attribute
{
    /// <summary>
    /// The priority at which requests to this connection will be made
    /// </summary>
    public virtual int Priority { get; set; } = 100;
}