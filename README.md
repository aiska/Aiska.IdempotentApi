# 🚀 Aiska.IdempotentApi

A .NET library for creating idempotent APIs, ensuring that requests are processed only once, even if received multiple times.

Making your APIs reliable and resilient with idempotency.

![License](https://img.shields.io/github/license/aiska/Aiska.IdempotentApi)
![GitHub stars](https://img.shields.io/github/stars/aiska/Aiska.IdempotentApi?style=social)
![GitHub forks](https://img.shields.io/github/forks/aiska/Aiska.IdempotentApi?style=social)
![GitHub issues](https://img.shields.io/github/issues/aiska/Aiska.IdempotentApi)
![GitHub pull requests](https://img.shields.io/github/issues-pr/aiska/Aiska.IdempotentApi)
![GitHub last commit](https://img.shields.io/github/last-commit/aiska/Aiska.IdempotentApi)
![GitHub contributors](https://img.shields.io/github/contributors/aiska/AIska.IdempotentApi)

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![NuGet Version](https://img.shields.io/nuget/v/Aiska.IdempotentApi)
![NuGet Downloads](https://img.shields.io/nuget/dt/Aiska.IdempotentApi)

## 📋 Table of Contents

- [About](#about)
- [Features](#features)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [Testing](#testing)
- [License](#license)
- [Support](#support)
- [Acknowledgments](#acknowledgments)

## About

Aiska.IdempotentApi is a .NET library designed to simplify the implementation of idempotent APIs. Idempotency is a crucial property for ensuring that an API request, when made multiple times, has the same effect as if it were made only once. This is particularly important in distributed systems where network issues can lead to requests being retried.

This library provides a straightforward way to wrap your API endpoints, ensuring that requests with the same idempotency key are processed only once. It handles the storage and retrieval of request results, preventing duplicate processing and ensuring data consistency.

The library is built using C# and targets .NET. It's designed to be lightweight and easy to integrate into existing .NET projects. The core architecture involves intercepting API requests, checking for an existing result based on the idempotency key, and either returning the stored result or processing the request and storing the result for future use.

## ✨ Features

- 🎯 **Idempotency Key Handling**: Automatically manages idempotency keys provided in request headers.
- ⚡ **Performance**: Optimized for minimal overhead, ensuring that idempotency checks don't significantly impact API response times.
- 🔒 **Concurrency**: Thread-safe implementation to handle concurrent requests safely.
- 🛠️ **Extensible**: Allows customization of storage mechanisms for idempotency keys and results.
- ⚙️ **Configurable**: Provides options to configure the behavior of the idempotency logic.

## 🚀 Quick Start

Install the NuGet package and add the necessary code to your ASP.NET Core pipeline.

```bash
Install-Package Aiska.IdempotentApi
```

```csharp
// In your Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    builder.Services.AddIdempotentApi(builder.Configuration);
    // Other service configurations
}
```

## 📦 Installation

### Prerequisites

- .Net 10
- An ASP.NET Core project

### NuGet Package

```bash
Install-Package Aiska.IdempotentApi
```

### From Source (Advanced)

```bash
# Clone the repository
git clone https://github.com/aiska/Aiska.IdempotentApi.git
cd Aiska.IdempotentApi

# Build the project
dotnet build
```

## 💻 Usage

### Basic Usage

1.  **Add the Idempotency Service**: In your `Program.cs` or `Startup.cs`, add the `AddIdempotency` service.

    ```csharp
    // Program.cs or Startup.cs
    builder.Services.AddIdempotency(); // For .NET 6+
    // OR
    services.AddIdempotency(); // For .NET Core 3.1 and .NET 5
    ```

2.  **Add the Idempotency Filter**: Add the `AddIdempotentFilter` Filter your Endpoint Minimal Api.

    ```csharp
    // Program.cs or Startup.cs
    todosApi.MapPost("/", async (Todo todo) =>
    {
        // simulate creation
        return Results.Created($"/todoitems/{todo.Id}", todo);
    }).AddIdempotentFilter();
    ```
    Or **Apply the `[Idempotent]` attribute to your controller actions **:

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Aiska.IdempotentApi;

    [ApiController]
    [Route("[controller]")]
    public class MyController : ControllerBase
    {
        [HttpPost]
        [Idempotent]
        public IActionResult MyAction([FromBody] MyRequest request)
        {
            // Your logic here
            return Ok(new { Message = "Request processed successfully" });
        }
    }

    public class MyRequest
    {
        public string Data { get; set; }
    }
    ```

4.  **Include an Idempotency-Key Header**:  Send an `Idempotency-Key` header with your API request. The value should be a unique identifier for the request.

    ```http
    POST /MyController/MyAction HTTP/1.1
    Content-Type: application/json
    Idempotency-Key: unique-request-id

    {
      "data": "some data"
    }
    ```

### Advanced Examples

For more advanced configuration options, see the [Configuration](#configuration) section.

## ⚙️ Configuration

### Idempotency Key Header Name

The default header name is `Idempotency-Key`. You can customize this:

```csharp
builder.Services.AddIdempotentApi(options =>
{
    options.KeyHeaderName = "X-Idempotency-Key";
    options.ExpirationFromMinutes = 5;
});
```

### Idempotency Cache expiration

The default Cache expiration name is 5. You can customize this:

```csharp
builder.Services.AddIdempotentApi(options =>
{
    options.ExpirationFromMinutes = 10;
});
```

### Storage Provider

By default, the library uses an in-memory storage provider.  For production environments, you'll want to implement a persistent storage provider (e.g., using Redis or a database).

```csharp
// Example (not a complete implementation)
public class CustomIdempotencyStore : IIdempotencyStore
{
    // Implement the IIdempotencyStore interface methods (GetAsync, SetAsync, etc.)
}

builder.Services.AddSingleton<IIdempotencyStore, CustomIdempotencyStore>();
```

## 📁 Project Structure

```
Aiska.IdempotentApi/
├── src/
│   ├── Aiska.IdempotentApi/
│   │   ├── Abtractions/             # Idempotent Abtractions
│   │   ├── Attributes/              # Idempotent Attribute
│   │   ├── Extensions/              # Extension methods for IServiceCollection and IApplicationBuilder
│   │   ├── Filters/                 # Filter to check for Idempotency
│   │   ├── Middleware/              # Idempotency Middleware
│   │   ├── Models/                  # Models for storing results
│   │   ├── Options/                 # Options for configuring the middleware
│   │   ├── Services/                # Idempotency Service and Store interfaces
│   │   └── ...
│   └── Aiska.IdempotentApi.csproj   # Project file
├── LICENSE                          # License file
└── README.md                        # This file
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) (placeholder - create this file) for details.

### Quick Contribution Steps

1.  🍴 Fork the repository
2.  🌟 Create your feature branch (`git checkout -b feature/AmazingFeature`)
3.  ✅ Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4.  📤 Push to the branch (`git push origin feature/AmazingFeature`)
5.  🔃 Open a Pull Request

### Development Setup

```bash
# Fork and clone the repo
git clone https://github.com/aiska/Aiska.IdempotentApi.git

# Navigate to the project directory
cd Aiska.IdempotentApi

# Build the project
dotnet build

# Run tests
dotnet test
```

### Code Style

-   Follow existing code conventions
-   Run `dotnet format` before committing
-   Add tests for new features
-   Update documentation as needed

## Testing

To run the tests:

```bash
dotnet test
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](docs/licence.md) file for details.

### License Summary

-   ✅ Commercial use
-   ✅ Modification
-   ✅ Distribution
-   ✅ Private use
-   ❌ Liability
-   ❌ Warranty

## 💬 Support

-   📧 **Email**:   [aiskahendra@gmail.com](aiskahendra@gmail.com)
-   🌐 **Website**: [aiskahendra.wordpress.com](https://aiskahendra.wordpress.com/)
-   🐛 **Issues**:  [GitHub Issues](https://github.com/aiska/Aiska.IdempotentApi/issues)

## 🙏 Acknowledgments

-   📚 **Libraries used**:
    -   [Microsoft.Extensions.Caching.Hybrid](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Hybrid/) - HybridCache library in ASP.NET Core (not yet ready to use).
-   👥 **Contributors**: Thanks to all [contributors](https://github.com/aiska/Aiska.IdempotentApi/contributors)
