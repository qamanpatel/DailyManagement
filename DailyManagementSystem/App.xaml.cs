using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using DailyManagementSystem.Core;
using DailyManagementSystem.Services;
using DailyManagementSystem.ViewModels;
using DailyManagementSystem.Services.Interfaces;
using DailyManagementSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using System;

namespace DailyManagementSystem
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Set up Dependency Injection
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<MainWindow>(provider => new MainWindow()
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddDbContext<DailyManagementSystem.Data.AppDbContext>(options => { }, ServiceLifetime.Transient);

            // Register Services
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IExpenseService, ExpenseService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IExportService, ExportService>();

            services.AddSingleton<IMessengerService, MessengerService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<ClientViewModel>();
            services.AddTransient<OrderViewModel>();
            services.AddTransient<PaymentViewModel>();
            services.AddTransient<ExpenseViewModel>();
            services.AddTransient<ReportViewModel>();
            
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => 
                (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow;

                // Initialize Services
                var notificationService = _serviceProvider.GetRequiredService<INotificationService>() as NotificationService;
                notificationService?.Initialize(mainWindow);

                var filePickerService = _serviceProvider.GetRequiredService<IFilePickerService>() as FilePickerService;
                filePickerService?.Initialize(mainWindow);

                // Initial navigation
                var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
                navigationService.NavigateTo<WelcomeViewModel>();

                // Ensure Database Created and Migrated
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DailyManagementSystem.Data.AppDbContext>();
                    dbContext.Database.EnsureCreated();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
