using BuildingBlocks.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

public interface ICachingHandlerService
{
    Task<T> HashGetAsync<T>(string cacheKey, string hashField);
    Task HashSetAsync(string cacheKey, string hashField, object value, TimeSpan? expiration = null);
    Task<bool> SortedSetAddAsync(string key, object value, double score);
    Task<bool> SortedSetRemoveAsync<T>(string key, T value);
    Task<double?> SortedSetScoreAsync<T>(string key, T value);
    Task<T> SortedSetGetLowestScoreAsync<T>(string key);
    Task<double> SortedSetIncrementAsync<T>(string key, object member, long value = 1);
    Task<double> SortedSetDecrementAsync<T>(string key, object member, long value = 1);
}

public class CachingHandlerService
(
    IConnectionMultiplexer _connectionMultiplexer
) : ICachingHandlerService
{
    private IDatabase Database => _connectionMultiplexer.GetDatabase();
    
    public async Task<T> HashGetAsync<T>(string cacheKey, string hashField)
    {
        var cachedResponse = await Database.HashGetAsync(cacheKey, hashField);
        return !string.IsNullOrEmpty(cachedResponse) 
            ? JsonConvert.DeserializeObject<T>(cachedResponse) 
            : default;
    }

    public async Task HashSetAsync(string cacheKey, string hashField, object value, TimeSpan? expiration = null)
    {
        await Database.HashSetAsync(cacheKey, hashField, Helpers.JsonHelper.Serialize(value));
        if (expiration.HasValue)
        {
            await Database.KeyExpireAsync(cacheKey, expiration.Value);
        }
    }
    
    public async Task<bool> SortedSetAddAsync(string key, object value, double score)
    {
        var entryBytes = Helpers.JsonHelper.Serialize(value);
        return await Database.SortedSetAddAsync(key, entryBytes, score);
    }

    public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
    {
        var entryBytes = Helpers.JsonHelper.Serialize(value);
        return await Database.SortedSetRemoveAsync(key, entryBytes);
    }
    
    public async Task<double?> SortedSetScoreAsync<T>(string key, T value)
    {
        var entryBytes = Helpers.JsonHelper.Serialize(value);
        return await Database.SortedSetScoreAsync(key, entryBytes);
    }
    
    public async Task<T> SortedSetGetLowestScoreAsync<T>(string key)
    {
        var entries = await Database.SortedSetRangeByRankWithScoresAsync(key, 0, 0).ConfigureAwait(false);
        var result = entries.Select(m => m.Element == RedisValue.Null ? default : JsonConvert.DeserializeObject<T>(m.Element)).FirstOrDefault();
        return result;
    }
    
    public async Task<double> SortedSetIncrementAsync<T>(string key, object member, long value = 1)
    { 
        var entryBytes = JsonConvert.SerializeObject(member);
        return await Database.SortedSetIncrementAsync(key, entryBytes, value).ConfigureAwait(false);
    }
    
    public async Task<double> SortedSetDecrementAsync<T>(string key, object member, long value = 1)
    {
        var entryBytes = JsonConvert.SerializeObject(member);
        return await Database.SortedSetDecrementAsync(key, entryBytes, value).ConfigureAwait(false);
    }
}