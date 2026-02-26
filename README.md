# MoM Notification Service

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A robust background service for automated Minutes of Meeting (MoM) notifications and reminders. This service runs scheduled jobs to notify department heads and team members about outstanding action items from meetings.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Jobs & Schedules](#jobs--schedules)
- [Project Structure](#project-structure)
- [Technologies](#technologies)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)

## 🎯 Overview

The MoM Notification Service is a .NET 8 Worker Service that automatically sends email reminders to relevant stakeholders about outstanding meeting action items. It features multi-level escalation, smart scheduling, and comprehensive logging to ensure critical tasks are tracked and completed on time.

## ✨ Features

- **📧 Automated Email Notifications**: Sends HTML-formatted email reminders using Gmail SMTP
- **🔄 Multi-Level Escalation System**:
  - **Level 1**: Daily reminders for department-specific MoMs (Mon-Fri at 6:00 PM)
  - **Level 2**: Weekly reminders for overdue items (Sunday at 9:00 PM)
- **🗓️ Smart Scheduling**: Department-specific schedule to distribute notification load
- **🚫 Anti-Spam Protection**: Prevents duplicate notifications on the same day
- **🧹 Automatic Cleanup**: Purges logs older than 6 months on the 1st of each month
- **🧪 Test Mode**: Safe testing environment with configurable test email recipients
- **📊 Severity Indicators**: Visual status indicators (🔴 Overdue, 🟡 Open, 🟢 On Progress)
- **👥 Smart Recipient Management**: Automatically identifies department heads, section heads, and PICs
- **📝 Comprehensive Logging**: Detailed logging for monitoring and troubleshooting

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   MoM Notification Service                   │
│                     (.NET 8 Worker)                          │
└─────────────────────────────────────────────────────────────┘
                              │
                 ┌────────────┼────────────┐
                 │            │            │
          ┌──────▼─────┐ ┌───▼────┐ ┌────▼─────┐
          │  Level 1   │ │ Level 2│ │ Cleanup  │
          │ Reminder   │ │Reminder│ │   Job    │
          │    Job     │ │  Job   │ │          │
          └──────┬─────┘ └───┬────┘ └────┬─────┘
                 │           │            │
                 └───────────┼────────────┘
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
    ┌─────▼─────┐     ┌─────▼──────┐    ┌─────▼─────┐
    │   Email   │     │    MoM     │    │   Log     │
    │  Service  │     │   Query    │    │Repository │
    │           │     │  Service   │    │           │
    └─────┬─────┘     └─────┬──────┘    └─────┬─────┘
          │                 │                  │
          │          ┌──────▼──────────────────▼───┐
          │          │      SQL Server Database    │
          │          │  • MoMs                     │
          └─────────►│  • MoMNotificationLogs      │
                     │  • vw_detail_karyawan_aktif │
                     └─────────────────────────────┘
```

## 📦 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2016+ or Azure SQL Database
- Gmail account with App Password enabled
- Access to the MoM database schema

## 🚀 Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/yourusername/MoM.NotificationService.git
   cd MoM.NotificationService
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

3. **Configure settings**:

   Copy the template files and fill in your actual configuration:

   ```bash
   cp appsettings.template.json appsettings.json
   cp appsettings.Development.template.json appsettings.Development.json
   ```

   Then edit `appsettings.json` and `appsettings.Development.json` with your actual credentials (see [Configuration](#configuration) section)

4. **Build the project**:

   ```bash
   dotnet build
   ```

5. **Run the service**:
   ```bash
   dotnet run
   ```

## ⚙️ Configuration

### Setup Configuration Files

The repository includes template files to help you get started:

- `appsettings.template.json` - Production configuration template
- `appsettings.Development.template.json` - Development configuration template

**⚠️ IMPORTANT**: The actual `appsettings.json` and `appsettings.Development.json` files are excluded from Git to protect your sensitive data (credentials, connection strings, etc.). You must create them locally from the templates.

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "NotificationSettings": {
    "IsTestMode": false,
    "TestEmail": "your-test-email@example.com"
  },
  "EmailSettings": {
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=DB_MYSECRETARY;User Id=sa;Password=your-password;TrustServerCertificate=True;"
  }
}
```

### Configuration Options

| Setting                               | Description                                            | Required                   |
| ------------------------------------- | ------------------------------------------------------ | -------------------------- |
| `NotificationSettings:IsTestMode`     | Enable test mode to send emails only to test recipient | Yes                        |
| `NotificationSettings:TestEmail`      | Test email address for test mode                       | Yes (if test mode enabled) |
| `EmailSettings:SenderEmail`           | Gmail account for sending emails                       | Yes                        |
| `EmailSettings:Password`              | Gmail App Password (not your regular password)         | Yes                        |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string                           | Yes                        |

### Gmail Setup

To use Gmail SMTP:

1. Enable 2-Factor Authentication on your Google account
2. Generate an [App Password](https://myaccount.google.com/apppasswords)
3. Use the App Password in `EmailSettings:Password`

## ⏰ Jobs & Schedules

### Level 1 Reminder Job

- **Schedule**: Monday - Friday at 6:00 PM (`0 0 18 ? * MON-FRI`)
- **Purpose**: Sends reminders to department heads about outstanding Level 1 MoMs
- **Recipients**:
  - TO: Department Head(s)
  - CC: Section Head(s)
- **Department Schedule**:
  - Monday: ENG
  - Tuesday: PPC, SPL
  - Wednesday: PUR, MTCE-ENG
  - Thursday: LOG, R&D
  - Friday: GA, HRD, ICT

### Level 2 Reminder Job

- **Schedule**: Sunday at 9:00 PM (`0 0 21 ? * SUN`)
- **Purpose**: Sends weekly escalation reminders for Level 2 MoMs
- **Recipients**:
  - TO: Department Head(s)
  - CC: Section Head(s), PICs (Person In Charge)
- **Features**:
  - Status breakdown (Overdue, Open, On Progress)
  - Severity icons based on status
  - PIC names included for accountability

### Notification Cleanup Job

- **Schedule**: 1st of every month at 2:00 AM (`0 0 2 1 * ?`)
- **Purpose**: Deletes notification logs older than 6 months
- **Benefits**: Maintains database performance and reduces storage

## 📁 Project Structure

```
MoM.NotificationService/
├── Jobs/
│   ├── Level1ReminderJob.cs       # Daily reminder for Level 1 MoMs
│   ├── Level2ReminderJob.cs       # Weekly reminder for Level 2 MoMs
│   └── NotificationCleanupJob.cs  # Log cleanup job
├── Services/
│   ├── EmailService.cs            # Email sending logic (Gmail SMTP)
│   └── MoMQueryService.cs         # Database queries for MoM data
├── Repositories/
│   └── NotificationLogRepository.cs # Notification log persistence
├── Templates/
│   ├── Level1EmailTemplate.cs     # HTML email template for Level 1
│   └── Level2EmailTemplate.cs     # HTML email template for Level 2
├── Dto/
│   └── MoMLevel2Dto.cs            # Data transfer objects
├── Program.cs                      # Service configuration & DI setup
├── appsettings.json               # Application settings
└── appsettings.Development.json   # Development-specific settings
```

## 🛠️ Technologies

- **Framework**: [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Job Scheduler**: [Quartz.NET 3.15.1](https://www.quartz-scheduler.net/)
- **Email**: [MailKit 4.15.0](https://github.com/jstedfast/MailKit)
- **Database Access**: [Dapper 2.1.66](https://github.com/DapperLib/Dapper)
- **Database Provider**: [Microsoft.Data.SqlClient 6.1.4](https://github.com/dotnet/SqlClient)
- **Hosting**: [Microsoft.Extensions.Hosting 9.0.8](https://docs.microsoft.com/dotnet/core/extensions/hosting)

## 💻 Development

### Prerequisites for Development

- Visual Studio 2022 / VS Code / Rider
- .NET 8 SDK
- SQL Server Management Studio (optional)

### Running in Development

1. Set `IsTestMode` to `true` in `appsettings.Development.json`
2. Configure your `TestEmail`
3. Run the service:
   ```bash
   dotnet run --environment Development
   ```

### Modifying Cron Schedules for Testing

For rapid testing, adjust the cron expressions in [Program.cs](Program.cs):

```csharp
// Test every minute
.WithCronSchedule("0 * * ? * *")

// Test every 10 seconds
.WithCronSchedule("0/10 * * ? * *")
```

### Debugging Tips

- Check logs for job execution: `🔥 Level X Reminder Job Triggered at...`
- Verify database connectivity by checking service startup logs
- Use test mode to avoid spamming real recipients
- Monitor the `MoMNotificationLogs` table for sent notifications

## 🚀 Deployment

### Windows Service

```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Install as Windows Service using sc
sc create MoMNotificationService binPath="C:\path\to\publish\MoM.NotificationService.exe"
sc start MoMNotificationService
```

### Linux Service (systemd)

```bash
# Publish for Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Create systemd service file
sudo nano /etc/systemd/system/mom-notification.service
```

Example service file:

```ini
[Unit]
Description=MoM Notification Service
After=network.target

[Service]
Type=notify
ExecStart=/path/to/MoM.NotificationService
Restart=always
User=www-data
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl enable mom-notification.service
sudo systemctl start mom-notification.service
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MoM.NotificationService.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MoM.NotificationService.dll"]
```

## 🔒 Security

### Protected Configuration Files

The following files contain sensitive information and are **excluded from Git**:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- Any `appsettings.*.json` files

### Best Practices

1. **Never commit sensitive data**: Use the provided `.template.json` files as examples
2. **Use environment variables**: For production deployments, consider using environment variables or Azure Key Vault
3. **Rotate credentials regularly**: Change passwords and connection strings periodically
4. **Use App Passwords**: For Gmail, always use App Passwords, never your actual account password
5. **Limit database permissions**: Use SQL accounts with minimum required permissions

### Setting Up Local Configuration

```bash
# Copy templates to create your local configuration
cp appsettings.template.json appsettings.json
cp appsettings.Development.template.json appsettings.Development.json

# Edit with your actual credentials (these files won't be committed)
nano appsettings.json
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

Developed with ❤️ by [Your Name/Team]

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub or contact the development team.

---

## ⚠️ Important Security Notice

This repository includes a `.gitignore` file that **excludes all configuration files containing sensitive data**. The actual `appsettings.json` files are **never committed to GitHub**.

**What IS included:**

- ✅ `appsettings.template.json` - Safe template without real credentials
- ✅ `appsettings.Development.template.json` - Safe development template

**What is EXCLUDED (protected):**

- 🚫 `appsettings.json` - Your actual configuration with real credentials
- 🚫 `appsettings.Development.json` - Your actual development configuration
- 🚫 All other `appsettings.*.json` files

**Setup instructions:** Copy the template files and add your real credentials locally. These files will remain on your machine only and won't be pushed to GitHub.

For production deployments, use environment variables, Azure Key Vault, or other secure secret management solutions instead of configuration files.
