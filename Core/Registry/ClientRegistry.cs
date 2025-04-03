using System.Net.Sockets;

namespace Core;

public static class ClientRegistry
{
    private static readonly IDictionary<Socket, AClientHandler> _servers = new Dictionary<Socket, AClientHandler>();
    public static bool TryAdd(Socket client, AClientHandler handler) => _servers.TryAdd(client, handler);
    public static void Remove(Socket client)
    {
        if (_servers.ContainsKey(client))
        {
            _servers.Remove(client);
        }
    }
    public static void Clear()
    {
        foreach (var proxy in _servers)
        {
            proxy.Value.Dispose();
        }
        _servers.Clear();
    }

    public static bool TryGetValue(Socket client, out AClientHandler proxy) => _servers.TryGetValue(client, out proxy);
}
