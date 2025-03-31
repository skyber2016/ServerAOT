using StackExchange.Redis;

namespace Core
{
    public class RedisService : IRedisService
    {
        private readonly ILogger _logger = LoggerManager.CreateLogger();
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _connection;
        public RedisService()
        {
            var config = ApplicationContext.Instance.AppConfig.RedisConfig;
            _connection = ConnectionMultiplexer.Connect($"{config.Host}:{config.Port},password={config.Auth}");
            _db = _connection.GetDatabase();
        }
        public IDatabase GetDatabase() => _db;

        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (expiry == TimeSpan.Zero)
            {
                return false;
            }
            var result = await _db.StringSetAsync(key, value, expiry);
            var exp = expiry.HasValue ? expiry.ToString() : "INF";
            if (result)
            {
                _logger.Info($"Setted key: {key} with expiry={exp}");
                return true;
            }
            _logger.Info($"Error when set key {key}");
            return false;
        }
        public async Task<string> GetAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }
        public async Task<bool> RemoveAsync(string key)
        {
            if (await _db.KeyDeleteAsync(key))
            {
                _logger.Info($"Removed {key}");
                return true;
            }
            _logger.Error($"Cannot remove {key}");
            return false;
        }

        public async Task DeleteKeysByPatternAsync(string pattern)
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray(); // Lấy danh sách key

            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys); // Xóa tất cả key
            }
        }


        public async Task<bool> KeyExistedAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        public async Task<bool> AcquireLockAsync(string key, string token, TimeSpan expiry)
        {
            return await _db.StringSetAsync(key, token, expiry, When.NotExists);
        }

        public async Task<bool> ReleaseLockAsync(string key, string token)
        {
            var currentValue = await _db.StringGetAsync(key);
            if (currentValue == token)
            {
                return await _db.KeyDeleteAsync(key);
            }
            return false;
        }
    }
}
