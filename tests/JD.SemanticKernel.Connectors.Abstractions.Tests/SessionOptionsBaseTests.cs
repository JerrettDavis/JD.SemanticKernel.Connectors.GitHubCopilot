using JD.SemanticKernel.Connectors.Abstractions;
using Xunit;

namespace JD.SemanticKernel.Connectors.Abstractions.Tests;

public class SessionOptionsBaseTests
{
    private sealed class TestOptions : SessionOptionsBase;

    [Fact]
    public void DangerouslyDisableSslValidation_DefaultsFalse()
    {
        var options = new TestOptions();
        Assert.False(options.DangerouslyDisableSslValidation);
    }

    [Fact]
    public void CustomEndpoint_DefaultsNull()
    {
        var options = new TestOptions();
        Assert.Null(options.CustomEndpoint);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new TestOptions
        {
            DangerouslyDisableSslValidation = true,
            CustomEndpoint = "https://custom.endpoint.com",
        };

        Assert.True(options.DangerouslyDisableSslValidation);
        Assert.Equal("https://custom.endpoint.com", options.CustomEndpoint);
    }
}
