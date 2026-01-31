using DailyManagementSystem.Core;

namespace DailyManagementSystem.Services
{
    public interface INavigationService
    {
        BaseViewModel? CurrentView { get; }
        event Action? CurrentViewChanged;
        void NavigateTo<T>() where T : BaseViewModel;
    }
}
