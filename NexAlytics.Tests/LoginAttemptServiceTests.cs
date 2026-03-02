using Microsoft.Extensions.Caching.Memory;
using NexAlytics.Application.Services;

namespace NexAlytics.Tests;

public class LoginAttemptServiceTests : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly LoginAttemptService _sut;
    private const string Email = "test@example.com";

    public LoginAttemptServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _sut = new LoginAttemptService(_cache);
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public void CheckLockout_NoAttempts_NotLocked()
    {
        var (isLocked, seconds) = _sut.CheckLockout(Email);

        Assert.False(isLocked);
        Assert.Equal(0, seconds);
    }

    [Fact]
    public void RecordFailure_BelowLimit_ReturnsRemainingAttempts()
    {
        var remaining = _sut.RecordFailure(Email);

        Assert.Equal(LoginAttemptService.GetMaxAttempts() - 1, remaining);
    }

    [Fact]
    public void RecordFailure_ReachesLimit_ReturnsZeroAndLocks()
    {
        int remaining = 0;
        for (int i = 0; i < LoginAttemptService.GetMaxAttempts(); i++)
            remaining = _sut.RecordFailure(Email);

        Assert.Equal(0, remaining);

        var (isLocked, seconds) = _sut.CheckLockout(Email);
        Assert.True(isLocked);
        Assert.True(seconds > 0);
    }

    [Fact]
    public void RecordSuccess_ClearsLockout()
    {
        for (int i = 0; i < LoginAttemptService.GetMaxAttempts(); i++)
            _sut.RecordFailure(Email);

        _sut.RecordSuccess(Email);

        var (isLocked, _) = _sut.CheckLockout(Email);
        Assert.False(isLocked);
    }

    [Fact]
    public void CheckLockout_IsCaseInsensitive()
    {
        for (int i = 0; i < LoginAttemptService.GetMaxAttempts(); i++)
            _sut.RecordFailure("USER@EXAMPLE.COM");

        var (isLocked, _) = _sut.CheckLockout("user@example.com");
        Assert.True(isLocked);
    }

    [Fact]
    public void GetMaxAttempts_ReturnsPositiveNumber()
    {
        Assert.True(LoginAttemptService.GetMaxAttempts() > 0);
    }

    [Fact]
    public void GetLockoutMinutes_ReturnsPositiveNumber()
    {
        Assert.True(LoginAttemptService.GetLockoutMinutes() > 0);
    }
}
