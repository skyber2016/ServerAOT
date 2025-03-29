
using StackExchange.Redis;

namespace Core
{
    public interface IRedisService
    {
        Task<bool> AcquireLockAsync(string key, string token, TimeSpan expiry);
        Task DeleteKeysByPatternAsync(string pattern);
        Task<string> GetAsync(string key);
        IDatabase GetDatabase();
        Task<bool> KeyExistedAsync(string key);
        Task<bool> ReleaseLockAsync(string key, string token);
        Task<bool> RemoveAsync(string key);
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
    }
}
