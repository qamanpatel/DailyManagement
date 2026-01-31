using System;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IMessengerService
    {
        event Action OrderChanged;
        void PublishOrderChanged();
    }
}
