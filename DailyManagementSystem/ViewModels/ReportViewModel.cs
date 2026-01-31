using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using DailyManagementSystem.Core;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly INavigationService _navigationService;
        private readonly IExportService _exportService;
        private readonly INotificationService _notificationService;
        private readonly IAuthService _authService;

        public bool IsAdmin => _authService.IsAdmin;

        private int? _fromMonth;
        private int? _fromYear;
        private int? _toMonth;
        private int? _toYear;

        private int _totalOrders;
        private decimal _totalOrderAmount;
        private decimal _totalIncome;
        private decimal _totalSpent;
        private decimal _pendingAmount;
        private decimal _netProfit;

        private ObservableCollection<OrderReportDto> _orderReports = new();
        private ObservableCollection<PaymentReportDto> _paymentReports = new();
        private ObservableCollection<ExpenseReportDto> _expenseReports = new();
        private ObservableCollection<CategoryExpenseDto> _categoryExpenseReports = new();
        private ObservableCollection<PersonExpenseDto> _personExpenseReports = new();

        private string _errorMessage = string.Empty;

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

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        public decimal TotalOrderAmount
        {
            get => _totalOrderAmount;
            set => SetProperty(ref _totalOrderAmount, value);
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        public decimal PendingAmount
        {
            get => _pendingAmount;
            set => SetProperty(ref _pendingAmount, value);
        }

        public decimal NetProfit
        {
            get => _netProfit;
            set => SetProperty(ref _netProfit, value);
        }

        public ObservableCollection<OrderReportDto> OrderReports
        {
            get => _orderReports;
            set => SetProperty(ref _orderReports, value);
        }

        public ObservableCollection<PaymentReportDto> PaymentReports
        {
            get => _paymentReports;
            set => SetProperty(ref _paymentReports, value);
        }

        public ObservableCollection<ExpenseReportDto> ExpenseReports
        {
            get => _expenseReports;
            set => SetProperty(ref _expenseReports, value);
        }

        public ObservableCollection<CategoryExpenseDto> CategoryExpenseReports
        {
            get => _categoryExpenseReports;
            set => SetProperty(ref _categoryExpenseReports, value);
        }

        public ObservableCollection<PersonExpenseDto> PersonExpenseReports
        {
            get => _personExpenseReports;
            set => SetProperty(ref _personExpenseReports, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        public ICommand LoadReportCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand DownloadReportCommand { get; }
        public ICommand BackCommand { get; }

        public ReportViewModel(IReportService reportService, INavigationService navigationService, IExportService exportService, INotificationService notificationService, IAuthService authService)
        {
            _reportService = reportService;
            _navigationService = navigationService;
            _exportService = exportService;
            _notificationService = notificationService;
            _authService = authService;

            LoadReportCommand = new RelayCommand(async _ => await ApplyFilterAsync());
            ClearFilterCommand = new RelayCommand(async _ => await ClearFilterAsync());
            DownloadReportCommand = new RelayCommand(async _ => await DownloadReportAsync());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());

            _ = LoadDataAsync();
        }

        private async Task ApplyFilterAsync()
        {
            if (FromMonth.HasValue && !FromYear.HasValue)
            {
                ErrorMessage = "Please select a Start Year.";
                return;
            }

            if (ToYear.HasValue && !FromYear.HasValue)
            {
                ErrorMessage = "Please select a Start Year if an End Year is provided.";
                return;
            }

            if (FromYear.HasValue && ToYear.HasValue)
            {
                if (FromYear.Value > ToYear.Value || (FromYear.Value == ToYear.Value && FromMonth.HasValue && ToMonth.HasValue && FromMonth.Value > ToMonth.Value))
                {
                    ErrorMessage = "Start period cannot be after end period.";
                    return;
                }
            }

            ErrorMessage = string.Empty;
            await LoadDataAsync();
            _notificationService.ShowSuccess("Filter Applied", "The report has been updated based on your range.");
        }

        private async Task ClearFilterAsync()
        {
            FromMonth = null;
            FromYear = null;
            ToMonth = null;
            ToYear = null;
            ErrorMessage = string.Empty;
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                // 1. Summary Metrics
                var summary = await _reportService.GetDashboardSummaryAsync(FromYear, FromMonth, ToYear, ToMonth);
                TotalOrders = summary.TotalOrders;
                TotalOrderAmount = summary.TotalOrderAmount;
                TotalIncome = summary.TotalReceivedAmount;
                TotalSpent = summary.TotalExpenses;
                PendingAmount = summary.TotalPendingAmount;
                NetProfit = TotalIncome - TotalSpent;

                // 2. Detailed Breakdown Collections
                var orderData = await _reportService.GetMonthlyOrderReportsAsync(FromYear, FromMonth, ToYear, ToMonth);
                OrderReports = new ObservableCollection<OrderReportDto>(orderData);

                var paymentData = await _reportService.GetMonthlyPaymentReportsAsync(FromYear, FromMonth, ToYear, ToMonth);
                PaymentReports = new ObservableCollection<PaymentReportDto>(paymentData);

                var expenseData = await _reportService.GetMonthlyExpenseReportsAsync(FromYear, FromMonth, ToYear, ToMonth);
                ExpenseReports = new ObservableCollection<ExpenseReportDto>(expenseData);

                var categoryData = await _reportService.GetMonthlyCategoryExpenseSummaryAsync(FromYear, FromMonth, ToYear, ToMonth);
                CategoryExpenseReports = new ObservableCollection<CategoryExpenseDto>(categoryData);

                var personData = await _reportService.GetMonthlyPersonExpenseSummaryAsync(FromYear, FromMonth, ToYear, ToMonth);
                PersonExpenseReports = new ObservableCollection<PersonExpenseDto>(personData);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading report: {ex.Message}";
            }
        }

        private async Task DownloadReportAsync()
        {
            if (IsDownloading) return;

            try
            {
                IsDownloading = true;
                ErrorMessage = string.Empty;

                // Prepare data
                var data = new ReportData
                {
                    StartYear = FromYear,
                    StartMonth = FromMonth,
                    EndYear = ToYear,
                    EndMonth = ToMonth,
                    Summary = await _reportService.GetDashboardSummaryAsync(FromYear, FromMonth, ToYear, ToMonth),
                    Orders = OrderReports,
                    Payments = PaymentReports,
                    Expenses = ExpenseReports,
                    CategorySummary = CategoryExpenseReports
                };

                // Determine path (Downloads folder)
                string downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadFolder))
                {
                    downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                string fromPart = FromYear.HasValue ? $"{FromYear.Value}_{(FromMonth.HasValue ? FromMonth.Value.ToString() : "All")}" : "Total";
                string toPart = ToYear.HasValue ? $"{ToYear.Value}_{(ToMonth.HasValue ? ToMonth.Value.ToString() : "All")}" : "Total";
                string fileName = $"Financial_Report_{fromPart}_to_{toPart}.pdf";
                string filePath = Path.Combine(downloadFolder, fileName);

                await _exportService.ExportReportToPdfAsync(filePath, data);

                _notificationService.ShowSuccess("Download Complete", $"Financial report saved to:\n{filePath}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error downloading report: {ex.Message}";
                _notificationService.ShowError("Download Failed", ex.Message);
            }
            finally
            {
                IsDownloading = false;
            }
        }
    }
}
