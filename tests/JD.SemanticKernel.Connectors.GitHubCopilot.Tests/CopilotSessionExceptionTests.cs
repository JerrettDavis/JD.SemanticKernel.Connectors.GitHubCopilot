using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotSessionExceptionTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        var ex = new CopilotSessionException();
        Assert.NotNull(ex);
    }

    [Fact]
    public void MessageConstructor_SetsMessage()
    {
        var ex = new CopilotSessionException("test message");
        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void InnerExceptionConstructor_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new CopilotSessionException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void IsInvalidOperationException()
    {
        var ex = new CopilotSessionException("test");
        Assert.IsAssignableFrom<InvalidOperationException>(ex);
    }
}
