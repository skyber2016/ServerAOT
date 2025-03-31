using System.Net.Sockets;
using System.Text;

namespace GameServer
{
    public static class PacketExtensions
    {
        public static async Task SystemNotification(this Socket client, string message)
        {
            var paddings = new List<byte> { 0xEC, 0x03, 0x00, 0xFF, 0xFF, 0x00, 0xD5, 0x07, 0x00, 0x00, 0x64, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x06, 0x53, 0x59, 0x53, 0x54, 0x45, 0x4D, 0x06, 0x4C, 0x75, 0x58, 0x75, 0x42, 0x75, 0x00 };
            var msg = Encoding.UTF8.GetBytes(message);
            paddings.Add((byte)msg.Length);
            paddings.AddRange(msg);
            using var mem = new MemoryStream();
            using var writer = new BinaryWriter(mem);
            writer.Write((ushort)(paddings.Count + 2 + 3));
            writer.Write(paddings.ToArray());
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
            writer.Write(byte.MinValue);
            var final = mem.ToArray();
            await client.SendAsync(final);
        }
    }
}
