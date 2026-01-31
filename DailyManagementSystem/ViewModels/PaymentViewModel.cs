using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DailyManagementSystem.Core;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class PaymentViewModel : BaseViewModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IClientService _clientService;
        private readonly IOrderService _orderService;
        private readonly IMessengerService _messengerService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public bool IsAdmin => _authService.IsAdmin;
        public bool IsFormEnabled => !IsEditing || IsAdmin;
        public bool CanViewAmount => !IsEditing || IsAdmin;

        private ObservableCollection<PaymentDto> _payments = new();
        private ObservableCollection<Client> _clients = new();
        private ObservableCollection<Order> _clientOrders = new();

        private Client? _selectedClient;
        private Order? _selectedOrder;
        private decimal _paymentAmount;
        private string? _bankName;
        private DateTimeOffset? _paymentDate = DateTimeOffset.Now;

        private decimal _totalOrderAmount;
        private decimal _totalPaidAmount;
        private decimal _pendingAmount;
        private decimal _orderRemainingAmount;

        private bool _isEditing;
        private PaymentDto? _selectedPaymentDto;
        private string _errorMessage = string.Empty;

        public ObservableCollection<PaymentDto> Payments
        {
            get => _payments;
            set => SetProperty(ref _payments, value);
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }

        public ObservableCollection<Order> ClientOrders
        {
            get => _clientOrders;
            set => SetProperty(ref _clientOrders, value);
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetProperty(ref _selectedClient, value))
                {
                    SelectedOrder = null;
                    _ = LoadClientDetailsAsync();
                }
            }
        }

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    _SelectOrderInternal(value);
                }
            }
        }

        private async void _SelectOrderInternal(Order? order)
        {
            await CalculateOrderBalanceAsync(order);
        }

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set => SetProperty(ref _paymentAmount, value);
        }

        public string? BankName
        {
            get => _bankName;
            set => SetProperty(ref _bankName, value);
        }

        public DateTimeOffset? PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        public decimal TotalOrderAmount
        {
            get => _totalOrderAmount;
            set => SetProperty(ref _totalOrderAmount, value);
        }

        public decimal TotalPaidAmount
        {
            get => _totalPaidAmount;
            set => SetProperty(ref _totalPaidAmount, value);
        }

        public decimal PendingAmount
        {
            get => _pendingAmount;
            set => SetProperty(ref _pendingAmount, value);
        }

        public decimal OrderRemainingAmount
        {
            get => _orderRemainingAmount;
            set => SetProperty(ref _orderRemainingAmount, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public PaymentDto? SelectedPaymentDto
        {
            get => _selectedPaymentDto;
            set
            {
                if (SetProperty(ref _selectedPaymentDto, value))
                {
                    if (value != null)
                    {
                        IsEditing = true;
                        PaymentAmount = value.AmountReceived;
                        BankName = value.BankName;
                        PaymentDate = value.PaymentDate;
                        SelectedClient = Clients.FirstOrDefault(c => c.ClientId == value.ClientId);
                        // Delay order selection to ensure ClientOrders are loaded
                        _ = SetSelectedOrderAsync(value.OrderId);
                    }
                }
            }
        }

        private async Task SetSelectedOrderAsync(int? orderId)
        {
            await Task.Delay(100); // Give time for ClientOrders to be populated from SelectedClient setter
            if (orderId.HasValue)
            {
                SelectedOrder = ClientOrders.FirstOrDefault(o => o.OrderId == orderId.Value);
            }
            else
            {
                SelectedOrder = null;
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        public PaymentViewModel(
            IPaymentService paymentService,
            IClientService clientService,
            IOrderService orderService,
            IMessengerService messengerService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _paymentService = paymentService;
            _clientService = clientService;
            _orderService = orderService;
            _messengerService = messengerService;
            _navigationService = navigationService;
            _authService = authService;

            SaveCommand = new RelayCommand(async _ => await SavePaymentAsync());
            DeleteCommand = new RelayCommand(async _ => await DeletePaymentAsync());
            ClearCommand = new RelayCommand(_ => ClearForm());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                Clients = new ObservableCollection<Client>(clients);

                var payments = await _paymentService.GetAllPaymentsAsync();
                var dtos = payments.Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    ClientId = p.ClientId,
                    OrderId = p.OrderId,
                    AmountReceived = p.AmountReceived,
                    BankName = p.BankName,
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt,
                    ClientName = p.Client?.ClientName ?? "Unknown",
                    OrderName = p.Order?.OrderName ?? (p.OrderId.HasValue ? $"Order #{p.OrderId}" : "Advance")
                });

                Payments = new ObservableCollection<PaymentDto>(dtos);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private async Task LoadClientDetailsAsync()
        {
            if (SelectedClient == null)
            {
                ClientOrders = new ObservableCollection<Order>();
                TotalOrderAmount = 0;
                TotalPaidAmount = 0;
                PendingAmount = 0;
                return;
            }

            try
            {
                // Load Orders for this client
                var orders = await _orderService.GetOrdersByClientAsync(SelectedClient.ClientId);
                ClientOrders = new ObservableCollection<Order>(orders);

                // Calculate Balances
                await CalculateClientBalanceAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading client details: {ex.Message}";
            }
        }

        private async Task CalculateClientBalanceAsync()
        {
            if (SelectedClient == null) return;

            var clientOrders = await _orderService.GetOrdersByClientAsync(SelectedClient.ClientId);
            var clientPayments = await _paymentService.GetPaymentsByClientAsync(SelectedClient.ClientId);

            TotalOrderAmount = clientOrders.Sum(o => o.OrderAmount);
            TotalPaidAmount = clientPayments.Sum(p => p.AmountReceived);
            PendingAmount = TotalOrderAmount - TotalPaidAmount;
        }

        private async Task CalculateOrderBalanceAsync(Order? order)
        {
            if (order == null)
            {
                OrderRemainingAmount = 0;
                return;
            }

            var orderPayments = await _paymentService.GetPaymentsByOrderAsync(order.OrderId);
            var paidForOrder = orderPayments.Sum(p => p.AmountReceived);
            
            // If editing, exclude our current payment from the "paid" total calculation to show actual quota left
            if (IsEditing && SelectedPaymentDto != null && SelectedPaymentDto.OrderId == order.OrderId)
            {
                paidForOrder -= SelectedPaymentDto.AmountReceived;
            }
            
            OrderRemainingAmount = order.OrderAmount - paidForOrder;
        }

        private async Task SavePaymentAsync()
        {
            if (IsEditing && !IsAdmin)
            {
                ErrorMessage = "Normal users cannot edit existing payments.";
                return;
            }

            if (IsEditing)
                await UpdatePaymentAsync();
            else
                await AddPaymentAsync();
        }

        private async Task UpdatePaymentAsync()
        {
            if (SelectedPaymentDto == null) return;
            
            if (PaymentAmount <= 0)
            {
                ErrorMessage = "Payment amount must be greater than zero.";
                return;
            }

            if (SelectedOrder != null && PaymentAmount > OrderRemainingAmount)
            {
                ErrorMessage = $"Payment exceeds remaining order balance ({OrderRemainingAmount:C}).";
                return;
            }

            try
            {
                var payment = new Payment
                {
                    PaymentId = SelectedPaymentDto.PaymentId,
                    ClientId = SelectedPaymentDto.ClientId, // Cannot change client
                    OrderId = SelectedOrder?.OrderId,
                    AmountReceived = PaymentAmount,
                    BankName = BankName,
                    PaymentDate = PaymentDate?.DateTime ?? DateTime.Now
                };

                await _paymentService.UpdatePaymentAsync(payment);

                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                await LoadClientDetailsAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task DeletePaymentAsync()
        {
            if (SelectedPaymentDto == null) return;
            if (!IsAdmin)
            {
                ErrorMessage = "Only administrators can delete payments.";
                return;
            }

            try
            {
                await _paymentService.DeletePaymentAsync(SelectedPaymentDto.PaymentId);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                await LoadClientDetailsAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting payment: {ex.Message}";
            }
        }

        private async Task AddPaymentAsync()
        {
            if (SelectedClient == null)
            {
                ErrorMessage = "Please select a client.";
                return;
            }

            if (PaymentAmount <= 0)
            {
                ErrorMessage = "Payment amount must be greater than zero.";
                return;
            }

            // Validation: Overpayment check if order selected
            if (SelectedOrder != null && PaymentAmount > OrderRemainingAmount)
            {
                ErrorMessage = $"Payment exceeds remaining order balance ({OrderRemainingAmount:C}).";
                return;
            }

            try
            {
                var payment = new Payment
                {
                    ClientId = SelectedClient.ClientId,
                    OrderId = SelectedOrder?.OrderId,
                    AmountReceived = PaymentAmount,
                    BankName = BankName,
                    PaymentDate = PaymentDate?.DateTime ?? DateTime.Now
                };

                await _paymentService.AddPaymentAsync(payment);

                _messengerService.PublishOrderChanged(); // Refresh dashboard
                await LoadDataAsync();
                await LoadClientDetailsAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void ClearForm()
        {
            SelectedPaymentDto = null;
            SelectedClient = null;
            SelectedOrder = null;
            PaymentAmount = 0;
            BankName = string.Empty;
            PaymentDate = DateTimeOffset.Now;
            IsEditing = false;
            ErrorMessage = string.Empty;
        }
    }
}
