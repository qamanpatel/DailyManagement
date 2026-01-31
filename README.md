# Daily Management System 🚀

A professional, high-performance desktop application built for businesses to streamline their daily operations, order management, and financial reporting.

![App Icon](DailyManagementSystem/Resources/icon.png)

## 🌟 Key Features

- **Dashboard & Analytics**: Real-time visualization of total orders, income, expenses, and net profit.
- **Client Management**: Maintain a detailed directory of clients with contact information and activity history.
- **Order Tracking**: Manage the full lifecycle of orders from "Pending" to "Delivered".
- **Financial Receipts**: Record and track payments against specific orders or clients.
- **Expense Management**: Track daily business expenditures categorized by personnel and type.
- **Client-Specific Reports**: Generate detailed financial snapshots for individual clients within custom date ranges.
- **Automated PDF Export**: Professional report generation using QuestPDF for high-quality documents.
- **Secure Authentication**: Role-based access control (Admin/User) using BCrypt password hashing.

## 🛠️ Tech Stack

- **Framework**: .NET 9
- **UI Framework**: Avalonia UI (Cross-platform XAML)
- **Database**: SQLite with Entity Framework Core
- **Architecture**: MVVM (Model-View-ViewModel)
- **Reports**: QuestPDF
- **Security**: BCrypt.Net

## 📸 Screenshots

| Splash Screen | Login Screen |
| :---: | :---: |
| ![Welcome](https://via.placeholder.com/400x250?text=Welcome+Splash) | ![Login](https://via.placeholder.com/400x250?text=Login+Screen) |

| Dashboard | Client Reports |
| :---: | :---: |
| ![Dashboard](https://via.placeholder.com/400x250?text=Dashboard+Analytics) | ![Reports](https://via.placeholder.com/400x250?text=Financial+Filtering) |

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/DailyManagementSystem.git
   ```
2. Navigate to the project directory:
   ```bash
   cd DailyManagementSystem/DailyManagementSystem
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

### Default Credentials
- **Admin**: `admin` / `admin123`
- **User**: `user` / `user123`

## 📦 Deployment (Windows)
To publish a standalone Windows executable:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---
Built with ❤️ by [Your Name](https://linkedin.com/in/your-profile)
