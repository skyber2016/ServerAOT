using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Core
{
    public class MemoryCacheService : IMemoryCacheService, IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly TimeSpan _cleanupInterval;
        private readonly CancellationTokenSource _cts = new();

        public MemoryCacheService(TimeSpan cleanupInterval)
        {
            _cleanupInterval = cleanupInterval;
            StartCleanupTask();
        }


        public void Set(string key, object value, TimeSpan ttl)
        {
            var expirationTime = DateTime.Now.Add(ttl);
            var item = new CacheItem(value, expirationTime);
            _cache[key] = item;
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public bool KeyExisted(string key) => _cache.ContainsKey(key);

        public void RemoveByPattern(string pattern)
        {
            var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");

            foreach (var key in _cache.Keys.Where(k => regex.IsMatch(k.ToString())).ToList())
            {
                _cache.TryRemove(key, out _);
            }
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            if (_cache.TryGetValue(key, out CacheItem item))
            {
                if (DateTime.UtcNow < item.Expiration)
                {
                    value = (T)item.Value;
                    return true;
                }
                else
                {
                    _cache.TryRemove(key, out _);
                }
            }
            value = default;
            return false;
        }

        private void StartCleanupTask()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_cleanupInterval, _cts.Token);
                        CleanupExpiredItems();
                    }
                    catch (TaskCanceledException) { }
                }
            }, _cts.Token);
        }

        private void CleanupExpiredItems()
        {
            var now = DateTime.Now;
            foreach (var kvp in _cache)
            {
                if (kvp.Value.Expiration < now)
                {
                    _cache.TryRemove(kvp.Key, out _);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        private class CacheItem
        {
            public object Value { get; }
            public DateTime Expiration { get; }

            public CacheItem(object value, DateTime expiration)
            {
                Value = value;
                Expiration = expiration;
            }
        }
    }
}
