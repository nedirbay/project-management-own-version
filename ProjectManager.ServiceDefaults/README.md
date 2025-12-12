# Project Manager Service Defaults

This library contains shared extension methods to configure common service behaviors standard in .NET Aspire applications.

## 📦 Components

- **OpenTelemetry**: Configures metrics and tracing for observability.
- **Health Checks**: Sets up standard health check endpoints (`/health`, `/alive`).
- **Service Discovery**: Default configuration for service discovery integration.

## 📝 Usage

These defaults are consumed by the `ProjectManager.API` and running services to ensure they all emit telemetry and health data in a consistent format that the Aspire Dashboard can digest.

Usage in `Program.cs`:

```csharp
builder.AddServiceDefaults();
```
