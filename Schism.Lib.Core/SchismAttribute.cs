using System.Diagnostics.CodeAnalysis;

namespace Schism.Lib.Core;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public abstract class SchismAttribute : Attribute
{
    public virtual int Priority { get; set; } = 100;
}