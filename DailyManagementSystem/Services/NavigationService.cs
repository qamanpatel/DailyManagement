using DailyManagementSystem.Core;

namespace DailyManagementSystem.Services
{
    public class NavigationService : INavigationService
    {
        private BaseViewModel? _currentView;
        private readonly Func<Type, BaseViewModel> _viewModelFactory;

        public BaseViewModel? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                CurrentViewChanged?.Invoke();
            }
        }

        public event Action? CurrentViewChanged;

        public NavigationService(Func<Type, BaseViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<T>() where T : BaseViewModel
        {
            BaseViewModel viewModel = _viewModelFactory.Invoke(typeof(T));
            CurrentView = viewModel;
        }
    }
}
