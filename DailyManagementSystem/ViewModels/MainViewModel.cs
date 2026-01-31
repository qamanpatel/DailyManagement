using DailyManagementSystem.Core;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public BaseViewModel? CurrentView => _navigationService.CurrentView;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.CurrentViewChanged += OnCurrentViewChanged;
            
            // Navigate to Home view on startup (to be implemented)
            // _navigationService.NavigateTo<HomeViewModel>(); 
        }

        private void OnCurrentViewChanged()
        {
            OnPropertyChanged(nameof(CurrentView));
        }
    }
}
