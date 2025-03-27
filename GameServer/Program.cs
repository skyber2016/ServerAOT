using Core;
using GameServer;

PacketRegistry.TryAdd(PacketType.LoginRequest, (client, proxy, channel) => new LoginRequestPacket(client, proxy, channel));
PacketRegistry.TryAdd(PacketType.LoginResponse, (client, proxy, channel) => new LoginRequestPacket(client, proxy, channel));