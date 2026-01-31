using System;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class MessengerService : IMessengerService
    {
        public event Action? OrderChanged;

        public void PublishOrderChanged()
        {
            OrderChanged?.Invoke();
        }
    }
}
