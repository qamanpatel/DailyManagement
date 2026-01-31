using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DailyManagementSystem.Core;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isBusy;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            LoginCommand = new RelayCommand(async _ => await LoginAsync());
            
            // Background task to ensure at least one admin exists
            _ = _authService.EnsureAdminUserExistsAsync();
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                bool success = await _authService.LoginAsync(Username, Password);

                if (success)
                {
                    _navigationService.NavigateTo<DashboardViewModel>();
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during login: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
