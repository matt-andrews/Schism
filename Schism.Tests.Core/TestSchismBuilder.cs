using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Schism.Lib.Core;
using System.Reflection;

namespace Schism.Tests.Core;
internal class TestSchismBuilder
{
    /// <summary>
    /// Test to ensure any property changes to <see cref="SchismBuilder"/> are also added to <see cref="SchismBuilder.CopyTo{T}"/>
    /// </summary>
    [Test]
    public void TestCopyToT()
    {
        SchismBuilder builder = new()
        {
            Configuration = Substitute.For<IConfiguration>(),
            LoadingAssembly = GetType().Assembly,
            Options = new(),
            Services = Substitute.For<IServiceCollection>()
        };

        SchismBuilder copy = builder.CopyTo<SchismBuilder>();

        PropertyInfo[] props = builder.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo prop in props)
        {
            if (prop.SetMethod is null)
            {
                continue;
            }
            object? left = prop.GetValue(builder);
            object? right = prop.GetValue(copy);
            Assert.That(left, Is.EqualTo(right), "Prop not matched: {0}", prop.Name);
        }
    }
    [Test]
    public void TestNamespaceDefinitionInOptions()
    {
        ServiceProvider collection = new ServiceCollection()
            .AddSchism(GetType().Assembly, new ConfigurationBuilder().Build(), TestHelpers.BuildOptions())
            .Build()
            .BuildServiceProvider();
        SchismOptions options = collection.GetRequiredService<SchismOptions>();
        Assert.That(options.Namespace, Is.EqualTo(GetType().Assembly.GetName().Name));
    }
}