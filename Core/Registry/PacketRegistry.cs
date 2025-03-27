using System.Net.Sockets;

namespace Core;

public static class PacketRegistry
{
    private static readonly IDictionary<PacketType, Func<Socket, Socket, Channel, APacketHandler>> _handlers = new Dictionary<PacketType, Func<Socket, Socket, Channel, APacketHandler>>();
    public static bool TryAdd(PacketType packetType, Func<Socket, Socket, Channel, APacketHandler> handler)
    {
        var logger = Logging.Write(typeof(PacketRegistry));
        if(_handlers.TryAdd(packetType, handler))
        {
            logger($"Added packet handler for {packetType}");
            return true;
        }
        else
        {
            logger($"Failed to add packet handler for {packetType}");
            return false;
        }
    }
    public static void Remove(PacketType packetType)
    {
        if (_handlers.ContainsKey(packetType))
        {
            _handlers.Remove(packetType);
        }
    }
    public static void Clear()
    {
        _handlers.Clear();
    }

    public static bool TryGetValue(PacketType packetType, out Func<Socket, Socket, Channel, APacketHandler> handler) => _handlers.TryGetValue(packetType, out handler);
}
