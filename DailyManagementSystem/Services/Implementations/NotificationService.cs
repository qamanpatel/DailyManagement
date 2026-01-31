using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private WindowNotificationManager? _notificationManager;

        public void Initialize(Visual visual)
        {
            _notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(visual))
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3
            };
        }

        public void ShowSuccess(string title, string message)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Success));
        }

        public void ShowError(string title, string message)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Error));
        }

        public void ShowInfo(string title, string message)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Information));
        }
    }
}
