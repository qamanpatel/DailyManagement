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
    public class ExpenseViewModel : BaseViewModel
    {
        private readonly IExpenseService _expenseService;
        private readonly IMessengerService _messengerService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public bool IsAdmin => _authService.IsAdmin;
        public bool IsFormEnabled => !IsEditing || IsAdmin;
        public bool CanViewAmount => !IsEditing || IsAdmin;

        private ObservableCollection<ExpenseDto> _expenses = new();
        private ObservableCollection<string> _categories = new();
        private ExpenseDto? _selectedExpense;

        private string _description = string.Empty;
        private string _category = "General";
        private string _spentBy = "Satyanam Patel";
        private string _newCategory = string.Empty;
        private decimal _amount;
        private DateTimeOffset? _spentDate = DateTimeOffset.Now;

        private decimal _totalMonthlyExpense;
        private string _errorMessage = string.Empty;
        private bool _isEditing;
        private bool _isAddingCategory;

        public ObservableCollection<string> StaffNames { get; } = new() 
        { 
            "Satyanam Patel", 
            "Sandeep Patel", 
            "Ram Tirath Patel" 
        };

        public ObservableCollection<ExpenseDto> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ExpenseDto? SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                if (SetProperty(ref _selectedExpense, value))
                {
                    if (value != null)
                    {
                        IsEditing = true;
                        Description = value.Description;
                        Category = value.Category;
                        SpentBy = value.SpentBy;
                        Amount = value.Amount;
                        SpentDate = value.SpentDate;
                    }
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string SpentBy
        {
            get => _spentBy;
            set => SetProperty(ref _spentBy, value);
        }

        public string NewCategory
        {
            get => _newCategory;
            set => SetProperty(ref _newCategory, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public DateTimeOffset? SpentDate
        {
            get => _spentDate;
            set => SetProperty(ref _spentDate, value);
        }

        public decimal TotalMonthlyExpense
        {
            get => _totalMonthlyExpense;
            set => SetProperty(ref _totalMonthlyExpense, value);
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

        public bool IsAddingCategory
        {
            get => _isAddingCategory;
            set => SetProperty(ref _isAddingCategory, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleAddCategoryCommand { get; }
        public ICommand ConfirmAddCategoryCommand { get; }

        public ExpenseViewModel(
            IExpenseService expenseService,
            IMessengerService messengerService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _expenseService = expenseService;
            _messengerService = messengerService;
            _navigationService = navigationService;
            _authService = authService;

            SaveCommand = new RelayCommand(async _ => await SaveExpenseAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteExpenseAsync());
            ClearCommand = new RelayCommand(_ => ClearForm());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            ToggleAddCategoryCommand = new RelayCommand(_ => IsAddingCategory = !IsAddingCategory);
            ConfirmAddCategoryCommand = new RelayCommand(_ => AddCategory());

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Categories
                var cats = await _expenseService.GetCategoriesAsync();
                var list = cats.ToList();
                // Ensure default categories are there
                string[] defaults = { "Fuel", "Delivery Charges", "Staff Salary", "Vehicle Maintenance", "Miscellaneous" };
                foreach (var d in defaults)
                {
                    if (!list.Contains(d)) list.Add(d);
                }
                Categories = new ObservableCollection<string>(list.OrderBy(c => c));

                // Expenses
                var expenses = await _expenseService.GetAllExpensesAsync();
                var dtos = expenses.Select(e => new ExpenseDto
                {
                    SpentId = e.SpentId,
                    Description = e.Description,
                    Category = e.Category,
                    SpentBy = e.SpentBy,
                    Amount = e.Amount,
                    SpentDate = e.SpentDate,
                    CreatedAt = e.CreatedAt
                });
                Expenses = new ObservableCollection<ExpenseDto>(dtos);

                // Summary (Current Month)
                var now = DateTime.Now;
                TotalMonthlyExpense = await _expenseService.GetTotalExpenseByMonthAsync(now.Year, now.Month);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategory)) return;
            if (!Categories.Contains(NewCategory))
            {
                Categories.Add(NewCategory);
                Category = NewCategory;
            }
            NewCategory = string.Empty;
            IsAddingCategory = false;
        }

        private async Task SaveExpenseAsync()
        {
            if (string.IsNullOrWhiteSpace(Description) || Amount <= 0 || SpentDate == null)
            {
                ErrorMessage = "Please fill all required fields.";
                return;
            }

            try
            {
                if (IsEditing && SelectedExpense != null)
                {
                    var expense = new DailySpent
                    {
                        SpentId = SelectedExpense.SpentId,
                        Description = Description,
                        Category = Category,
                        SpentBy = SpentBy,
                        Amount = Amount,
                        SpentDate = SpentDate.Value.DateTime
                    };
                    await _expenseService.UpdateExpenseAsync(expense);
                }
                else
                {
                    var expense = new DailySpent
                    {
                        Description = Description,
                        Category = Category,
                        SpentBy = SpentBy,
                        Amount = Amount,
                        SpentDate = SpentDate.Value.DateTime
                    };
                    await _expenseService.AddExpenseAsync(expense);
                }

                _messengerService.PublishOrderChanged(); // Refresh dashboard
                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task DeleteExpenseAsync()
        {
            if (SelectedExpense == null) return;
            if (!IsAdmin)
            {
                ErrorMessage = "Only administrators can delete expenses.";
                return;
            }

            try
            {
                await _expenseService.DeleteExpenseAsync(SelectedExpense.SpentId);
                _messengerService.PublishOrderChanged();
                await LoadDataAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            SelectedExpense = null;
            Description = string.Empty;
            Category = "General";
            SpentBy = "Satyanam Patel";
            Amount = 0;
            SpentDate = DateTimeOffset.Now;
            IsEditing = false;
            ErrorMessage = string.Empty;
        }
    }
}
