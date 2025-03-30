using System.Net.Sockets;
using Core;

namespace GameServer
{
    public interface INotificationService
    {
        Task SendNotification(Socket client, NotificationType type, uint accountId, string message, CancellationToken cancellation = default);
    }
}
