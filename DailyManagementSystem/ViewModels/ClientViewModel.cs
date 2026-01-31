using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyManagementSystem.Core;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.ViewModels
{
    public class ClientViewModel : BaseViewModel
    {
        private readonly IClientService _clientService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public bool IsAdmin => _authService.IsAdmin;
        public bool IsFormEnabled => !IsEditing || IsAdmin;
        public bool CanViewContact => !IsEditing || IsAdmin;

        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
        private Client? _selectedClient;
        private string _clientName = string.Empty;
        private string _phone = string.Empty;
        private string _address = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isEditing;

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetProperty(ref _selectedClient, value))
                {
                    if (value != null)
                    {
                        // Enter Edit Mode
                        ClientName = value.ClientName;
                        Phone = value.Phone ?? string.Empty;
                        Address = value.Address ?? string.Empty;
                        IsEditing = true;
                    }
                }
            }
        }

        public string ClientName
        {
            get => _clientName;
            set
            {
                SetProperty(ref _clientName, value);
                ErrorMessage = string.Empty; // Clear error on typing
            }
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }

        public ClientViewModel(IClientService clientService, INavigationService navigationService, IAuthService authService)
        {
            _clientService = clientService;
            _navigationService = navigationService;
            _authService = authService;

            SaveCommand = new RelayCommand(async _ => await SaveClientAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteClientAsync());
            ClearCommand = new RelayCommand(_ => ClearForm());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());

            _ = LoadClientsAsync();
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                var clients = await _clientService.GetAllActiveClientsAsync();
                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading clients: {ex.Message}";
            }
        }

        private async Task SaveClientAsync()
        {
            if (IsEditing && !IsAdmin)
            {
                ErrorMessage = "Normal users cannot edit existing client details.";
                return;
            }

            try
            {
                if (IsEditing && SelectedClient != null)
                {
                    // Update
                    var clientToUpdate = SelectedClient;
                    clientToUpdate.ClientName = ClientName;
                    clientToUpdate.Phone = Phone;
                    clientToUpdate.Address = Address;
                    
                    await _clientService.UpdateClientAsync(clientToUpdate);
                }
                else
                {
                    // Add
                    var newClient = new Client
                    {
                        ClientName = ClientName,
                        Phone = Phone,
                        Address = Address
                    };

                    await _clientService.CreateClientAsync(newClient);
                }

                ClearForm();
                await LoadClientsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null) return;
            if (!IsAdmin)
            {
                ErrorMessage = "Only administrators can delete clients.";
                return;
            }

            try
            {
                await _clientService.DeleteClientAsync(SelectedClient.ClientId);
                ClearForm();
                await LoadClientsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting client: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            SelectedClient = null;
            ClientName = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            ErrorMessage = string.Empty;
            IsEditing = false;
        }
    }
}
