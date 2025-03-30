

namespace Core
{
    public interface IMemoryCacheService
    {
        bool KeyExisted(string key);
        void Remove(string key);
        void RemoveByPattern(string pattern);
        void Set(string key, object value, TimeSpan ttl);
        bool TryGetValue<T>(string key, out T value);
    }
}
