# Project Manager API

The backend service for the Project Manager application, built with ASP.NET Core and .NET 9.

## 🏗 Architecture

The API follows a Clean Architecture inspired approach with:

- **Controllers**: Handle HTTP requests.
- **DTOs**: Data Transfer Objects for API contracts.
- **Repositories**: Data access layer.
- **Services/Helpers**: Business logic helpers.

## 🔑 Key Features

- **Authentication**: User management and JWT-based security.
- **Yumus (Tasks)**: Comprehensive task management including:
  - CRUD operations
  - Subtasks
  - Comments
  - Attachments
  - Status & Priority tracking
- **Project Structure**: Workspaces -> Projects -> Tasks hierarchy.
- **Reports**: Daily reporting functionality.

## ⚙️ Configuration

Configuration is handled via `appsettings.json`. Ensure your database connection strings and JWT settings are properly configured in development or via user secrets.

## 🚀 Running the API

You can run the API independently:

```bash
dotnet run
```

However, it is recommended to run it via the **`ProjectManager.AppHost`** project for full integration with the frontend.
