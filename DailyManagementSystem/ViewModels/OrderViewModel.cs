using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using Avalonia.Media.Imaging;
using DailyManagementSystem.Core;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services;

namespace DailyManagementSystem.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IClientService _clientService;
        private readonly IMessengerService _messengerService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;
        private readonly IExportService _exportService;
        private readonly INotificationService _notificationService;
        private readonly IFilePickerService _filePickerService;

        private ObservableCollection<OrderDto> _allOrders = new();
        private ObservableCollection<OrderDto> _orders = new();
        private ObservableCollection<Client> _clients = new();
        private ObservableCollection<Client> _filterClients = new();

        private Client? _selectedFilterClient;
        private Client? _selectedClient;
        private OrderDto? _selectedOrder;

        // --- Core Fields ---
        private string? _orderName;
        private decimal _orderAmount;
        private DateTimeOffset? _orderDate = DateTimeOffset.Now;
        
        // --- Work Order Specific Fields ---
        private string? _size;
        private string? _uom = "PC";
        private int _quantity = 1;
        private string? _materialNo;
        private string? _costingLayer;
        private string? _color = "White";

        private string? _materialSpec = "Mix Of Resin and Fiber Glass with required Iron Armature.";
        private string? _paintSpec = "Deco Antique Colour with high gloss P U polish.";
        private string? _qualitySpec = "Standard and all weatherproof.";
        private string? _workNatureSpec = "Fully hand made clay modelling based.";
        private string? _durabilitySpec = "Long-Lasting.";

        private DateTimeOffset? _modelingLastDate;
        private DateTimeOffset? _fiberStartDate;

        private string? _orderBy;
        private string? _modelingBy;
        private string? _fiberBy;

        // --- Image Upload ---
        private string? _imagePath;
        private string? _selectedLocalImagePath;
        private Bitmap? _imagePreview;

        private bool _isEditing;
        private bool _isClientActive = true;
        private string _errorMessage = string.Empty;

        public bool IsAdmin => _authService.IsAdmin;
        public bool IsFormEnabled => !IsEditing || IsAdmin;
        public bool CanViewAmount => !IsEditing || IsAdmin;

        public ObservableCollection<OrderDto> Orders { get => _orders; set => SetProperty(ref _orders, value); }
        public ObservableCollection<Client> Clients { get => _clients; set => SetProperty(ref _clients, value); }
        public ObservableCollection<Client> FilterClients { get => _filterClients; set => SetProperty(ref _filterClients, value); }

        public Client? SelectedFilterClient
        {
            get => _selectedFilterClient;
            set { if (SetProperty(ref _selectedFilterClient, value)) FilterOrders(); }
        }

        public OrderDto? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    if (value != null)
                    {
                        IsEditing = true;
                        OrderName = value.OrderName;
                        OrderAmount = value.OrderAmount;
                        OrderDate = value.OrderDate;
                        SelectedClient = Clients.FirstOrDefault(c => c.ClientId == value.ClientId);
                        
                        Size = value.Size;
                        Uom = value.UOM;
                        Quantity = value.Quantity;
                        MaterialNo = value.MaterialNo;
                        CostingLayer = value.CostingLayer;
                        Color = value.Color;

                        MaterialSpec = value.MaterialSpec ?? MaterialSpec;
                        PaintSpec = value.PaintSpec ?? PaintSpec;
                        QualitySpec = value.QualitySpec ?? QualitySpec;
                        WorkNatureSpec = value.WorkNatureSpec ?? WorkNatureSpec;
                        DurabilitySpec = value.DurabilitySpec ?? DurabilitySpec;

                        ModelingLastDate = value.ModelingLastDate;
                        FiberStartDate = value.FiberStartDate;

                        OrderBy = value.OrderBy;
                        ModelingBy = value.ModelingBy;
                        FiberBy = value.FiberBy;

                        ImagePath = value.ImagePath;
                        LoadImagePreview(value.ImagePath);

                        CheckClientStatus(value);
                    }
                }
            }
        }

        public Client? SelectedClient { get => _selectedClient; set => SetProperty(ref _selectedClient, value); }
        public string? OrderName { get => _orderName; set => SetProperty(ref _orderName, value); }
        public decimal OrderAmount { get => _orderAmount; set => SetProperty(ref _orderAmount, value); }
        public DateTimeOffset? OrderDate { get => _orderDate; set => SetProperty(ref _orderDate, value); }

        public string? Size { get => _size; set => SetProperty(ref _size, value); }
        public string? Uom { get => _uom; set => SetProperty(ref _uom, value); }
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        public string? MaterialNo { get => _materialNo; set => SetProperty(ref _materialNo, value); }
        public string? CostingLayer { get => _costingLayer; set => SetProperty(ref _costingLayer, value); }
        public string? Color { get => _color; set => SetProperty(ref _color, value); }

        public string? MaterialSpec { get => _materialSpec; set => SetProperty(ref _materialSpec, value); }
        public string? PaintSpec { get => _paintSpec; set => SetProperty(ref _paintSpec, value); }
        public string? QualitySpec { get => _qualitySpec; set => SetProperty(ref _qualitySpec, value); }
        public string? WorkNatureSpec { get => _workNatureSpec; set => SetProperty(ref _workNatureSpec, value); }
        public string? DurabilitySpec { get => _durabilitySpec; set => SetProperty(ref _durabilitySpec, value); }

        public DateTimeOffset? ModelingLastDate { get => _modelingLastDate; set => SetProperty(ref _modelingLastDate, value); }
        public DateTimeOffset? FiberStartDate { get => _fiberStartDate; set => SetProperty(ref _fiberStartDate, value); }

        public string? OrderBy { get => _orderBy; set => SetProperty(ref _orderBy, value); }
        public string? ModelingBy { get => _modelingBy; set => SetProperty(ref _modelingBy, value); }
        public string? FiberBy { get => _fiberBy; set => SetProperty(ref _fiberBy, value); }

        public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }
        public Bitmap? ImagePreview { get => _imagePreview; set => SetProperty(ref _imagePreview, value); }

        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
        public bool IsClientActive { get => _isClientActive; set => SetProperty(ref _isClientActive, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        public ICommand SaveCommand { get; }
        public ICommand MarkDeliveredCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DownloadWorkOrderCommand { get; }
        public ICommand SelectImageCommand { get; }

        public OrderViewModel(
            IOrderService orderService, 
            IClientService clientService, 
            IMessengerService messengerService,
            INavigationService navigationService,
            IAuthService authService,
            IExportService exportService,
            INotificationService notificationService,
            IFilePickerService filePickerService)
        {
            _orderService = orderService;
            _clientService = clientService;
            _messengerService = messengerService;
            _navigationService = navigationService;
            _authService = authService;
            _exportService = exportService;
            _notificationService = notificationService;
            _filePickerService = filePickerService;

            SaveCommand = new RelayCommand(async _ => await SaveOrderAsync());
            MarkDeliveredCommand = new RelayCommand(async _ => await MarkDeliveredAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteOrderAsync());
            ClearCommand = new RelayCommand(_ => ClearForm());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            DownloadWorkOrderCommand = new RelayCommand(async _ => await DownloadWorkOrderAsync());
            SelectImageCommand = new RelayCommand(async _ => await SelectImageAsync());

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var allClients = await _clientService.GetAllClientsAsync();
                var activeClients = await _clientService.GetAllActiveClientsAsync();
                Clients = new ObservableCollection<Client>(activeClients);
                FilterClients = new ObservableCollection<Client>(allClients); 

                var orders = await _orderService.GetAllOrdersAsync();
                var dtos = orders.Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    ClientId = o.ClientId,
                    ClientName = o.Client?.ClientName ?? "Unknown",
                    IsClientActive = o.Client?.IsActive ?? false,
                    OrderName = o.OrderName,
                    OrderAmount = o.OrderAmount,
                    OrderDate = o.OrderDate,
                    DeliveredDate = o.DeliveredDate,
                    Status = o.Status,
                    Size = o.Size,
                    UOM = o.UOM,
                    Quantity = o.Quantity,
                    MaterialNo = o.MaterialNo,
                    CostingLayer = o.CostingLayer,
                    Color = o.Color,
                    MaterialSpec = o.MaterialSpec,
                    PaintSpec = o.PaintSpec,
                    QualitySpec = o.QualitySpec,
                    WorkNatureSpec = o.WorkNatureSpec,
                    DurabilitySpec = o.DurabilitySpec,
                    ModelingLastDate = o.ModelingLastDate,
                    FiberStartDate = o.FiberStartDate,
                    OrderBy = o.OrderBy,
                    ModelingBy = o.ModelingBy,
                    FiberBy = o.FiberBy,
                    ImagePath = o.ImagePath
                }).ToList();

                _allOrders = new ObservableCollection<OrderDto>(dtos);
                FilterOrders();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private void FilterOrders()
        {
            if (SelectedFilterClient == null)
            {
                Orders = new ObservableCollection<OrderDto>(_allOrders);
            }
            else
            {
                var filtered = _allOrders.Where(o => o.ClientId == SelectedFilterClient.ClientId);
                Orders = new ObservableCollection<OrderDto>(filtered);
            }
        }

        private void CheckClientStatus(OrderDto order)
        {
            IsClientActive = order.IsClientActive;
            ErrorMessage = !IsClientActive ? "Note: This client is inactive. Order is read-only." : string.Empty;
        }

        private async Task SelectImageAsync()
        {
            try
            {
                var path = await _filePickerService.PickImageAsync();
                if (path == null) return;

                var fileInfo = new FileInfo(path);
                if (fileInfo.Length > 2 * 1024 * 1024) // 2MB
                {
                    _notificationService.ShowError("File Too Large", "Please select an image smaller than 2MB.");
                    return;
                }

                _selectedLocalImagePath = path;
                using var stream = File.OpenRead(path);
                ImagePreview = new Bitmap(stream);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Error Selecting Image", ex.Message);
            }
        }

        private string? ProcessAndSaveImage()
        {
            if (string.IsNullOrEmpty(_selectedLocalImagePath)) return ImagePath;

            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyManagementSystem", "Images");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(_selectedLocalImagePath)}";
                var destPath = Path.Combine(folder, fileName);

                File.Copy(_selectedLocalImagePath, destPath, true);

                // Optionally delete old image if it was local
                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    try { File.Delete(ImagePath); } catch { /* Ignore delete errors */ }
                }

                return destPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return ImagePath;
            }
        }

        private void LoadImagePreview(string? path)
        {
            ImagePreview = null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            try
            {
                using var stream = File.OpenRead(path);
                ImagePreview = new Bitmap(stream);
            }
            catch { /* Ignore load errors */ }
        }

        private async Task SaveOrderAsync()
        {
            if (IsEditing && !IsAdmin)
            {
                ErrorMessage = "Normal users cannot edit existing orders.";
                return;
            }

            if (IsEditing) await UpdateOrderAsync();
            else await AddOrderAsync();
        }

        private async Task AddOrderAsync()
        {
            if (SelectedClient == null) { ErrorMessage = "Please select a client."; return; }
            if (OrderAmount <= 0) { ErrorMessage = "Amount must be greater than zero."; return; }

            try
            {
                var finalImagePath = ProcessAndSaveImage();

                var newOrder = new Order
                {
                    ClientId = SelectedClient.ClientId,
                    OrderName = OrderName,
                    OrderAmount = OrderAmount,
                    OrderDate = OrderDate?.DateTime ?? DateTime.Now,
                    Status = "Pending",
                    Size = Size,
                    UOM = Uom,
                    Quantity = Quantity,
                    MaterialNo = MaterialNo,
                    CostingLayer = CostingLayer,
                    Color = Color,
                    MaterialSpec = MaterialSpec,
                    PaintSpec = PaintSpec,
                    QualitySpec = QualitySpec,
                    WorkNatureSpec = WorkNatureSpec,
                    DurabilitySpec = DurabilitySpec,
                    ModelingLastDate = ModelingLastDate?.DateTime,
                    FiberStartDate = FiberStartDate?.DateTime,
                    OrderBy = OrderBy,
                    ModelingBy = ModelingBy,
                    FiberBy = FiberBy,
                    ImagePath = finalImagePath
                };

                await _orderService.CreateOrderAsync(newOrder);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                ClearForm();
                _notificationService.ShowSuccess("Order Created", "The new order and specifications have been saved.");
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        private async Task UpdateOrderAsync()
        {
            if (SelectedOrder == null) return;
            if (!SelectedOrder.IsClientActive) { ErrorMessage = "Cannot edit order for inactive client."; return; }

            try
            {
                var finalImagePath = ProcessAndSaveImage();

                var orderToUpdate = new Order
                {
                    OrderId = SelectedOrder.OrderId,
                    ClientId = SelectedOrder.ClientId,
                    OrderName = OrderName,
                    OrderAmount = OrderAmount,
                    OrderDate = OrderDate?.DateTime ?? DateTime.Now,
                    DeliveredDate = SelectedOrder.DeliveredDate,
                    Status = SelectedOrder.Status,
                    Size = Size,
                    UOM = Uom,
                    Quantity = Quantity,
                    MaterialNo = MaterialNo,
                    CostingLayer = CostingLayer,
                    Color = Color,
                    MaterialSpec = MaterialSpec,
                    PaintSpec = PaintSpec,
                    QualitySpec = QualitySpec,
                    WorkNatureSpec = WorkNatureSpec,
                    DurabilitySpec = DurabilitySpec,
                    ModelingLastDate = ModelingLastDate?.DateTime,
                    FiberStartDate = FiberStartDate?.DateTime,
                    OrderBy = OrderBy,
                    ModelingBy = ModelingBy,
                    FiberBy = FiberBy,
                    ImagePath = finalImagePath
                };

                await _orderService.UpdateOrderAsync(orderToUpdate);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                ClearForm();
                _notificationService.ShowSuccess("Order Updated", "Specifications have been updated successfully.");
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        private async Task MarkDeliveredAsync()
        {
            if (SelectedOrder == null || SelectedOrder.Status == "Delivered") return;
            try
            {
                await _orderService.MarkOrderAsDeliveredAsync(SelectedOrder.OrderId, DateTime.Now);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        private async Task DeleteOrderAsync()
        {
            if (SelectedOrder == null) return;
            if (!IsAdmin) { ErrorMessage = "Only administrators can delete orders."; return; }

            try
            {
                // Delete local image if exists
                if (!string.IsNullOrEmpty(SelectedOrder.ImagePath) && File.Exists(SelectedOrder.ImagePath))
                {
                    try { File.Delete(SelectedOrder.ImagePath); } catch { }
                }

                await _orderService.DeleteOrderAsync(SelectedOrder.OrderId);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex) { ErrorMessage = $"Error deleting order: {ex.Message}"; }
        }

        private async Task DownloadWorkOrderAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                string downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(downloadFolder)) downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string safeOrderName = OrderName?.Replace(" ", "_") ?? "Order";
                string fileName = $"WorkOrder_{SelectedOrder.OrderId}_{safeOrderName}.pdf";
                string filePath = Path.Combine(downloadFolder, fileName);

                var fullOrder = await _orderService.GetOrderByIdAsync(SelectedOrder.OrderId);
                if (fullOrder == null) return;

                await _exportService.ExportWorkOrderToPdfAsync(filePath, fullOrder);
                _notificationService.ShowSuccess("Work Order Generated", $"PDF saved to:\n{filePath}");
            }
            catch (Exception ex) 
            { 
                _notificationService.ShowError("Export Failed", ex.Message);
            }
        }

        private void ClearForm()
        {
            SelectedOrder = null;
            SelectedClient = null;
            OrderName = string.Empty;
            OrderAmount = 0;
            OrderDate = DateTimeOffset.Now;

            Size = string.Empty;
            Uom = "PC";
            Quantity = 1;
            MaterialNo = string.Empty;
            CostingLayer = string.Empty;
            Color = "White";

            ModelingLastDate = null;
            FiberStartDate = null;
            OrderBy = string.Empty;
            ModelingBy = string.Empty;
            FiberBy = string.Empty;

            ImagePath = null;
            _selectedLocalImagePath = null;
            ImagePreview = null;

            IsEditing = false;
            IsClientActive = true;
            ErrorMessage = string.Empty;
        }
    }
}
