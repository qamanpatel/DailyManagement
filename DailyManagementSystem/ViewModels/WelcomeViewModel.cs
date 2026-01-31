using System.Threading.Tasks;
using System.Windows.Input;
using DailyManagementSystem.Core;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class WelcomeViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public ICommand EnterCommand { get; }

        public WelcomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            EnterCommand = new RelayCommand(_ => EnterDashboard());
            
            // Auto-transition
            _ = InitiateSequenceAsync();
        }

        private async Task InitiateSequenceAsync()
        {
            await Task.Delay(3000); // 3 seconds delay
            EnterDashboard();
        }

        private void EnterDashboard()
        {
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
