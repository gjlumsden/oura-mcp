using System.ComponentModel;
using System.Reflection;
using FluentAssertions;
using ModelContextProtocol.Server;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class ToolAttributeTests
{
    private static readonly Type[] AllToolTypes =
    [
        typeof(SleepTools),
        typeof(ActivityTools),
        typeof(ReadinessTools),
        typeof(BodyTools),
        typeof(WellnessTools),
        typeof(ProfileTools),
        typeof(TagTools),
        typeof(DiagnosticTools),
    ];

    [Fact]
    public void AllToolClasses_HaveMcpServerToolTypeAttribute()
    {
        foreach (var toolType in AllToolTypes)
        {
            toolType.Should().BeDecoratedWith<McpServerToolTypeAttribute>(
                because: $"{toolType.Name} must be discoverable by the MCP SDK");
        }
    }

    [Fact]
    public void AllToolMethods_HaveMcpServerToolAttribute()
    {
        foreach (var toolType in AllToolTypes)
        {
            var publicMethods = toolType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            publicMethods.Should().NotBeEmpty(
                because: $"{toolType.Name} should expose at least one tool method");

            foreach (var method in publicMethods)
            {
                method.Should().BeDecoratedWith<McpServerToolAttribute>(
                    because: $"{toolType.Name}.{method.Name} must be registered as an MCP tool");
            }
        }
    }

    [Fact]
    public void AllToolMethods_HaveDescriptionAttribute()
    {
        foreach (var toolType in AllToolTypes)
        {
            var publicMethods = toolType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var method in publicMethods)
            {
                method.Should().BeDecoratedWith<DescriptionAttribute>(
                    because: $"{toolType.Name}.{method.Name} must have a description for MCP tool discovery");
            }
        }
    }

    [Fact]
    public void AllToolMethods_DescriptionAttributes_AreNotEmpty()
    {
        foreach (var toolType in AllToolTypes)
        {
            var publicMethods = toolType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var method in publicMethods)
            {
                var description = method.GetCustomAttribute<DescriptionAttribute>();
                description?.Description.Should().NotBeNullOrWhiteSpace(
                    because: $"{toolType.Name}.{method.Name} description must be meaningful");
            }
        }
    }
}
