using Microsoft.Extensions.Caching.Distributed;

namespace MyProject.Infrastructure.Caching;

internal static class CacheOptions
{
    public static DistributedCacheEntryOptions Create(TimeSpan? expiration) =>
        expiration is null
            ? new DistributedCacheEntryOptions()
            : new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration };
}
