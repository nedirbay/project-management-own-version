# Project Manager Solution

This is a complete Project Management solution built using modern technologies, orchestrated with **.NET Aspire**.

## 📂 Project Structure

- **`ProjectManager.AppHost`**: The entry point for the solution. It uses .NET Aspire to orchestrate and run both the backend API and the frontend application together.
- **`ProjectManager.API`**: The backend RESTful API built with ASP.NET Core (.NET 9). It handles all data logic, authentication, and resource management.
- **`project-managament`**: The frontend application built with Vue.js 3, TypeScript, and Vite. It provides a modern user interface for managing projects and tasks.
- **`ProjectManager.ServiceDefaults`**: Contains shared default configurations for logging, metrics, and health checks (OpenTelemetry) used across .NET services.

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK**
- **Node.js** (v18 or later)
- **Docker Desktop** (Required for .NET Aspire orchestration)

### Running the Application

1. Open the solution in your preferred IDE (Visual Studio / VS Code).
2. Set **`ProjectManager.AppHost`** as the startup project.
3. Run the application (F5).

Or via command line:

```bash
cd ProjectManager.AppHost
dotnet run
```

This will invoke the **Aspire Dashboard**, where you can see:

- The running API service (Default port: random or configured).
- The running Vue frontend (proxied via Aspire).

## 🛠 Features

- **Workspaces & Projects**: Organize work into workspaces and projects.
- **Task Management (Yumus)**: Create, assign, and track tasks (Yumuş) with priorities, statuses, and deadlines.
- **Subtasks & Comments**: detailed task tracking.
- **Daily Reports**: Track progress over time.
- **Dashboard**: High-level overview of project status.

## 🔧 Technologies

- **Backend**: .NET 9, ASP.NET Core Web API
- **Frontend**: Vue 3, Vite, Tailwind/CSS
- **Orchestration**: .NET Aspire
