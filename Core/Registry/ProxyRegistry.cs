using System.Net.Sockets;

namespace Core;

public static class ProxyRegistry
{
    private static readonly IDictionary<Socket, AProxyHandler> _proxies = new Dictionary<Socket, AProxyHandler>();
    public static bool TryAdd(Socket client, AProxyHandler proxy) => _proxies.TryAdd(client, proxy);
    public static void Remove(Socket client)
    {
        if (_proxies.ContainsKey(client))
        {
            _proxies[client].Dispose();
            _proxies.Remove(client);
        }
    }
    public static void Clear()
    {
        foreach (var proxy in _proxies)
        {
            proxy.Value.Dispose();
        }
        _proxies.Clear();
    }

    public static bool TryGetValue(Socket client, out AProxyHandler proxy) => _proxies.TryGetValue(client, out proxy);
}
