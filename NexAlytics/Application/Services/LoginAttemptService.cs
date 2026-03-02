using Microsoft.Extensions.Caching.Memory;

namespace NexAlytics.Application.Services;

public class LoginAttemptService
{
    private readonly IMemoryCache _cache;

    private const int MaxAttempts = 5;
    private const int LockoutMinutes = 15;

    public LoginAttemptService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>Returns (isLocked, secondsRemaining). Call before attempting login.</summary>
    public (bool IsLocked, int SecondsRemaining) CheckLockout(string email)
    {
        if (_cache.TryGetValue(CacheKey(email), out LoginAttemptRecord? record) && record?.LockedUntil != null)
        {
            if (record.LockedUntil > DateTimeOffset.UtcNow)
            {
                var seconds = (int)(record.LockedUntil.Value - DateTimeOffset.UtcNow).TotalSeconds;
                return (true, seconds);
            }
        }
        return (false, 0);
    }

    /// <summary>Registers a failed attempt. Returns remaining attempts before lockout (0 = just locked).</summary>
    public int RecordFailure(string email)
    {
        var key = CacheKey(email);
        var record = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(LockoutMinutes + 5);
            return new LoginAttemptRecord();
        })!;

        record.Attempts++;

        if (record.Attempts >= MaxAttempts)
            record.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);

        _cache.Set(key, record, TimeSpan.FromMinutes(LockoutMinutes + 5));

        return Math.Max(0, MaxAttempts - record.Attempts);
    }

    /// <summary>Clears the failure counter on successful login.</summary>
    public void RecordSuccess(string email) => _cache.Remove(CacheKey(email));

    public static int GetMaxAttempts() => MaxAttempts;
    public static int GetLockoutMinutes() => LockoutMinutes;

    private static string CacheKey(string email) => $"login_fail:{email.Trim().ToLower()}";
}

file class LoginAttemptRecord
{
    public int Attempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
}
