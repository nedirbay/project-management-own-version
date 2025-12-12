# Project Manager AppHost

This is the **.NET Aspire** orchestration project for the Project Manager solution.

## 🎯 Purpose

This project is responsible for:

1. **Orchestrating Services**: It starts both the `ProjectManager.API` (Backend) and `project-managament` (Frontend).
2. **Service Discovery**: It manages connection strings and service endpoints (e.g., passing the API URL to the Frontend via `VITE_API_URL`).
3. **Dashboard**: It provides the Aspire Dashboard to monitor logs, traces, and environment variables for all running services.

## ▶️ Usage

Set this project as the **Startup Project** in Visual Studio or run via CLI:

```bash
dotnet run
```

This ensures that all parts of the application startup in the correct order with the necessary configuration injected.
