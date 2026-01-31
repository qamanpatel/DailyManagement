using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DailyManagementSystem.Core;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly INavigationService _navigationService;
        private readonly IMessengerService _messengerService;
        private readonly INotificationService _notificationService;
        private readonly IAuthService _authService;

        public bool IsAdmin => _authService.IsAdmin;
        public string CurrentUserName => _authService.CurrentUser?.Username ?? "Guest";

        private int? _selectedMonth;
        private int? _selectedYear;
        private int _totalOrders;
        private decimal _totalOrderAmount;
        private decimal _totalReceivedAmount;
        private decimal _totalPendingAmount;
        private decimal _totalExpenses;
        private decimal _profit;
        private string _errorMessage = string.Empty;

        public ObservableCollection<int?> Months { get; } = new ObservableCollection<int?>();
        public ObservableCollection<int?> Years { get; } = new ObservableCollection<int?>();

        public int? SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
        }

        public int? SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        public decimal TotalOrderAmount
        {
            get => IsAdmin ? _totalOrderAmount : 0;
            set => SetProperty(ref _totalOrderAmount, value);
        }

        public decimal TotalReceivedAmount
        {
            get => IsAdmin ? _totalReceivedAmount : 0;
            set => SetProperty(ref _totalReceivedAmount, value);
        }

        public decimal TotalPendingAmount
        {
            get => IsAdmin ? _totalPendingAmount : 0;
            set => SetProperty(ref _totalPendingAmount, value);
        }

        public decimal TotalExpenses
        {
            get => IsAdmin ? _totalExpenses : 0;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal Profit
        {
            get => IsAdmin ? _profit : 0;
            set => SetProperty(ref _profit, value);
        }

        public ICommand ApplyFilterCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddClientCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand AddExpenseCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand LogoutCommand { get; }

        public DashboardViewModel(
            IReportService reportService, 
            INavigationService navigationService,
            IMessengerService messengerService,
            INotificationService notificationService,
            IAuthService authService)
        {
            _reportService = reportService;
            _navigationService = navigationService;
            _messengerService = messengerService;
            _notificationService = notificationService;
            _authService = authService;

            // Subscribe to updates
            _messengerService.OrderChanged += async () => await LoadDashboardDataAsync();

            // Initialize Months (null for All)
            Months.Add(null);
            for (int i = 1; i <= 12; i++) Months.Add(i);

            // Initialize Years (null for All)
            Years.Add(null);
            var currentYear = DateTime.Now.Year;
            for (int i = currentYear - 5; i <= currentYear + 1; i++) Years.Add(i);

            // Defaults: No filter by default as requested
            _selectedMonth = null;
            _selectedYear = null;

            ApplyFilterCommand = new RelayCommand(async _ => await ApplyFilterAsync());
            RefreshCommand = new RelayCommand(async _ => await ClearFilterAsync());
            
            AddClientCommand = new RelayCommand(_ => _navigationService.NavigateTo<ClientViewModel>());
            AddOrderCommand = new RelayCommand(_ => _navigationService.NavigateTo<OrderViewModel>());
            AddPaymentCommand = new RelayCommand(_ => _navigationService.NavigateTo<PaymentViewModel>());
            AddExpenseCommand = new RelayCommand(_ => _navigationService.NavigateTo<ExpenseViewModel>());
            GenerateReportCommand = new RelayCommand(_ => _navigationService.NavigateTo<ReportViewModel>());
            LogoutCommand = new RelayCommand(_ => Logout());

            _ = LoadDashboardDataAsync();
        }

        private async Task ApplyFilterAsync()
        {
            if (SelectedMonth.HasValue && !SelectedYear.HasValue)
            {
                ErrorMessage = "Please select a Year to filter by Month.";
                _notificationService.ShowError("Filter Error", ErrorMessage);
                return;
            }

            ErrorMessage = string.Empty;
            await LoadDashboardDataAsync();
            _notificationService.ShowSuccess("Filter Applied", "Dashboard metrics updated.");
        }

        private async Task ClearFilterAsync()
        {
            SelectedMonth = null;
            SelectedYear = null;
            ErrorMessage = string.Empty;
            await LoadDashboardDataAsync();
            _notificationService.ShowInfo("Dashboard Reset", "Showing all lifetime data.");
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var summary = await _reportService.GetDashboardSummaryAsync(SelectedYear, SelectedMonth);
                
                TotalOrders = summary.TotalOrders;
                TotalOrderAmount = summary.TotalOrderAmount;
                TotalReceivedAmount = summary.TotalReceivedAmount;
                TotalPendingAmount = summary.TotalPendingAmount;
                TotalExpenses = summary.TotalExpenses;
                Profit = summary.Profit;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard: {ex.Message}";
                Console.WriteLine(ErrorMessage);
            }
        }

        private void Logout()
        {
            _authService.Logout();
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
