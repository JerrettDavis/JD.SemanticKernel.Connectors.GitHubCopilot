using JD.SemanticKernel.Connectors.Abstractions;
using Xunit;

namespace JD.SemanticKernel.Connectors.Abstractions.Tests;

public class ModelInfoTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var model = new ModelInfo("gpt-4o", "GPT-4o", "openai");
        Assert.Equal("gpt-4o", model.Id);
        Assert.Equal("GPT-4o", model.Name);
        Assert.Equal("openai", model.Provider);
    }

    [Fact]
    public void Provider_DefaultsToNull()
    {
        var model = new ModelInfo("gpt-4o", "GPT-4o");
        Assert.Null(model.Provider);
    }

    [Fact]
    public void Records_SupportEquality()
    {
        var a = new ModelInfo("gpt-4o", "GPT-4o", "openai");
        var b = new ModelInfo("gpt-4o", "GPT-4o", "openai");
        Assert.Equal(a, b);
    }
}
