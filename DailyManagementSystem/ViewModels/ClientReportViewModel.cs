using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services; // Correct namespace for INavigationService
using DailyManagementSystem.Core;

namespace DailyManagementSystem.ViewModels
{
    public class ClientReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IClientService _clientService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Client> _clients = new();
        private Client? _selectedClient;
        private ClientSpecificReportDto? _reportData;

        // Filters matching ReportViewModel
        private int? _fromMonth;
        private int? _fromYear;
        private int? _toMonth;
        private int? _toYear;

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set => SetProperty(ref _selectedClient, value);
        }

        public ClientSpecificReportDto? ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        // Filter Collections
        public List<int?> Months { get; } = new List<int?> { null }.Concat(Enumerable.Range(1, 12).Cast<int?>()).ToList();
        public List<int?> Years { get; } = new List<int?> { null }.Concat(Enumerable.Range(DateTime.Now.Year - 5, 10).Cast<int?>()).ToList();

        public int? FromMonth
        {
            get => _fromMonth;
            set => SetProperty(ref _fromMonth, value);
        }

        public int? FromYear
        {
            get => _fromYear;
            set => SetProperty(ref _fromYear, value);
        }

        public int? ToMonth
        {
            get => _toMonth;
            set => SetProperty(ref _toMonth, value);
        }

        public int? ToYear
        {
            get => _toYear;
            set => SetProperty(ref _toYear, value);
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand GoBackCommand { get; }

        public ClientReportViewModel(IReportService reportService, IClientService clientService, INavigationService navigationService)
        {
            _reportService = reportService;
            _clientService = clientService;
            _navigationService = navigationService;
            
            GenerateReportCommand = new RelayCommand(async _ => await GenerateReport());
            GoBackCommand = new RelayCommand(_ => GoBack());

            LoadClients();
        }

        private async void LoadClients()
        {
            var clients = await _clientService.GetAllClientsAsync();
            Clients = new ObservableCollection<Client>(clients);
        }

        private async Task GenerateReport()
        {
            if (SelectedClient == null) return;

            // Validate Range similar to ReportViewModel if needed, 
            // but for now relying on service to handle or basic checks.
            // ReportService.GetDateRange handles nulls gracefully.

            ReportData = await _reportService.GetClientSpecificReportAsync(
                SelectedClient.ClientId, 
                FromYear, 
                FromMonth, 
                ToYear, 
                ToMonth);
        }

        private void GoBack()
        {
             _navigationService.NavigateTo<DashboardViewModel>();
        }
    }
}
