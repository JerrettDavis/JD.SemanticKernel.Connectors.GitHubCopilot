using JD.SemanticKernel.Connectors.Abstractions;
using Xunit;

namespace JD.SemanticKernel.Connectors.Abstractions.Tests;

public class SessionCredentialsTests
{
    [Fact]
    public void Token_ReturnsProvidedValue()
    {
        var creds = new SessionCredentials("test-token");
        Assert.Equal("test-token", creds.Token);
    }

    [Fact]
    public void ExpiresAt_DefaultsToNull()
    {
        var creds = new SessionCredentials("test-token");
        Assert.Null(creds.ExpiresAt);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenNoExpiry()
    {
        var creds = new SessionCredentials("test-token");
        Assert.False(creds.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenExpiryInFuture()
    {
        var creds = new SessionCredentials("test-token", DateTimeOffset.UtcNow.AddHours(1));
        Assert.False(creds.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenExpiryInPast()
    {
        var creds = new SessionCredentials("test-token", DateTimeOffset.UtcNow.AddMinutes(-5));
        Assert.True(creds.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WithinSafetyMargin()
    {
        // Token expires in 15 seconds — within the 30-second safety margin
        var creds = new SessionCredentials("test-token", DateTimeOffset.UtcNow.AddSeconds(15));
        Assert.True(creds.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_JustOutsideSafetyMargin()
    {
        var creds = new SessionCredentials("test-token", DateTimeOffset.UtcNow.AddSeconds(60));
        Assert.False(creds.IsExpired);
    }
}
