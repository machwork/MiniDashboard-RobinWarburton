# Mini Dashboard Application

A WPF + API "Mini Dashboard" application demonstrating clean, modular .NET solution architecture with MVVM pattern and RESTful API.

## 📋 Project Overview

This solution consists of:
- **MiniDashboard.Api** - ASP.NET Core Web API (v8.0) backend
- **MiniDashboard.App** - WPF desktop application with MVVM pattern
- **MiniDashboard.Tests** - Unit tests for business logic
- **MiniDashboard.IntegrationTests** - Integration tests for API endpoints

## 🏗️ Architecture

### Backend (Web API)
- **Layered Architecture**: Controllers → Services → Repository
- **Dependency Injection**: All services registered via DI container
- **In-Memory Storage**: Uses in-memory list or local JSON file for persistence
- **RESTful API**: Standard CRUD endpoints with proper HTTP status codes
- **Swagger/OpenAPI**: API documentation available at `/swagger`

### Frontend (WPF Application)
- **MVVM Pattern**: Strict separation of concerns (no code-behind logic)
- **Dependency Injection**: Services registered and injected via DI
- **Async/Await**: All API calls use async/await patterns
- **ObservableCollection**: For data binding and UI updates
- **Commands**: ICommand implementation for user actions
- **Error Handling**: Graceful handling of loading and error states

## 📁 Project Structure

```
MiniDashboard/
├── MiniDashboard.Api/
│   ├── Controllers/          # API Controllers
│   ├── Services/             # Business logic services
│   ├── Repositories/         # Data access layer
│   ├── Models/               # Domain models
│   └── DTOs/                 # Data Transfer Objects
├── MiniDashboard.App/
│   ├── Models/               # Data models
│   ├── ViewModels/           # MVVM ViewModels
│   ├── Views/                # XAML views
│   ├── Services/             # API client services
│   └── Commands/             # ICommand implementations
├── MiniDashboard.Tests/      # Unit tests
└── MiniDashboard.IntegrationTests/  # Integration tests
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- (Optional) Postman or similar tool for API testing

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MiniDashboard
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   cd MiniDashboard.Api
   dotnet run
   ```
   The API will be available at `https://localhost:5001` or `http://localhost:5000`
   Swagger UI: `https://localhost:5001/swagger`

5. **Run the WPF Application**
   ```bash
   cd MiniDashboard.App
   dotnet run
   ```

### Running Tests

**Run all tests:**
```bash
dotnet test
```

**Run unit tests only:**
```bash
dotnet test MiniDashboard.Tests
```

**Run integration tests only:**
```bash
dotnet test MiniDashboard.IntegrationTests
```

## 📡 API Endpoints

The API provides the following endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/items` | Get all items |
| GET | `/api/items/{id}` | Get item by ID |
| GET | `/api/items/search?query=xyz` | Search items |
| POST | `/api/items` | Create new item |
| PUT | `/api/items/{id}` | Update existing item |
| DELETE | `/api/items/{id}` | Delete item |

### Example API Request

**Create Item:**
```http
POST /api/items
Content-Type: application/json

{
  "name": "Sample Item",
  "description": "Item description",
  "category": "Category Name"
}
```

**Search Items:**
```http
GET /api/items/search?query=sample
```

## 🧪 Testing Strategy

### Unit Tests
- Test ViewModels and business logic
- Mock external dependencies using Moq
- Use AAA pattern (Arrange, Act, Assert)
- Include both positive and negative test scenarios

### Integration Tests
- Test API endpoints end-to-end
- Use `Microsoft.AspNetCore.Mvc.Testing` for in-memory testing
- Verify HTTP status codes and response content
- Test all CRUD operations

## 🎯 Implementation Checklist

### Backend (API)
- [ ] Create Item model/DTO
- [ ] Implement IItemRepository interface
- [ ] Implement ItemRepository (in-memory or JSON file)
- [ ] Create IItemService interface
- [ ] Implement ItemService with business logic
- [ ] Create ItemsController with CRUD endpoints
- [ ] Add search functionality
- [ ] Configure dependency injection in Program.cs
- [ ] Add proper error handling and HTTP status codes
- [ ] Write unit tests for service layer
- [ ] Write integration tests for controllers

### Frontend (WPF)
- [ ] Create Item model matching API DTO
- [ ] Create IApiService interface
- [ ] Implement ApiService with HttpClient
- [ ] Create base ViewModel with INotifyPropertyChanged
- [ ] Create MainViewModel with ObservableCollection<Item>
- [ ] Implement ICommand for Add, Edit, Delete, Search
- [ ] Create MainWindow.xaml with data binding
- [ ] Add loading indicators
- [ ] Add error message display
- [ ] Configure dependency injection in App.xaml.cs
- [ ] Write unit tests for ViewModels

## 📦 NuGet Packages

### API Project
- ASP.NET Core Web API (included in template)
- Swashbuckle.AspNetCore (Swagger/OpenAPI)

### WPF Project
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Http

### Test Projects
- xUnit (testing framework)
- Moq (mocking framework)
- FluentAssertions (assertions)
- Microsoft.AspNetCore.Mvc.Testing (integration tests)

## 🔧 Configuration

### API Configuration
The API uses `appsettings.json` for configuration. Update the base URL if needed:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### WPF Configuration
The WPF app should configure the API base URL. Consider using `appsettings.json` or a configuration class.

## 🎨 Design Patterns Used

- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling and testability
- **MVVM Pattern**: Separation of UI and business logic
- **Command Pattern**: User action handling
- **Service Layer Pattern**: Business logic encapsulation

## 📝 Code Quality Standards

- Follow SOLID principles
- Use async/await for I/O operations
- Implement proper error handling
- Include XML documentation comments
- Use meaningful variable and method names
- Keep methods focused and single-purpose

## 🚧 Next Steps

1. Implement the Item model/DTO
2. Create repository with in-memory storage
3. Implement service layer with business logic
4. Create API controller with CRUD endpoints
5. Build WPF ViewModels and Views
6. Implement API client service
7. Add error handling and loading states
8. Write comprehensive tests

## 📄 License

This project is created for demonstration purposes.

## 👤 Author

[Your Name]

---

**Note**: This is a starter template. You need to implement the actual business logic, models, and UI according to the requirements.



