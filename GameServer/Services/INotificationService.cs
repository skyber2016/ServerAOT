using Core;
using System.Net.Sockets;

namespace GameServer
{
    public interface INotificationService
    {
        Task SendNotification(Socket client, NotificationType type, uint accountId, string message, CancellationToken cancellation = default);
    }
}
