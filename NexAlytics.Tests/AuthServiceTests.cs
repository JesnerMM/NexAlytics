using NexAlytics.Application.Services;

namespace NexAlytics.Tests;

public class AuthServiceTests
{
    [Fact]
    public void ComputeHash_SameInput_ReturnsSameHash()
    {
        var hash1 = AuthService.ComputeHash("Admin123!");
        var hash2 = AuthService.ComputeHash("Admin123!");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_DifferentInputs_ReturnDifferentHashes()
    {
        var hash1 = AuthService.ComputeHash("Admin123!");
        var hash2 = AuthService.ComputeHash("OtherPassword");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsLowerHex()
    {
        var hash = AuthService.ComputeHash("test");

        Assert.Equal(hash, hash.ToLower());
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public void ComputeHash_KnownValue_MatchesSeedHash()
    {
        // Hash of "Admin123!" stored in seed data
        const string expected = "3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121";
        var actual = AuthService.ComputeHash("Admin123!");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ComputeHash_EmptyString_ReturnsValidHash()
    {
        var hash = AuthService.ComputeHash(string.Empty);

        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
    }
}
