# Mini Dashboard Application

A full-stack WPF + ASP.NET Core Web API "Mini Dashboard" application demonstrating clean, modular .NET solution architecture with MVVM pattern, RESTful API, comprehensive testing, and local caching.

## 📋 Project Overview

This solution is a complete enterprise-grade application consisting of:
- **MiniDashboard.Api** - ASP.NET Core Web API (v8.0) backend with RESTful endpoints
- **MiniDashboard.App** - WPF desktop application with MVVM pattern and Material Design-inspired UI
- **MiniDashboard.Tests** - Comprehensive test suite including:
  - Unit tests for ViewModels, Services, Controllers, and Repositories
  - Integration tests for API endpoints
  - **UI Automation tests** using TestStack.White
- **MiniDashboard.IntegrationTests** - Additional integration tests

## ✨ Key Features

- ✅ Full CRUD operations (Create, Read, Update, Delete)
- ✅ Real-time search functionality
- ✅ **Local caching** with both in-memory and file-based persistence
- ✅ Offline mode support
- ✅ Async/await throughout for responsive UI
- ✅ Modern, clean UI with loading states and error handling
- ✅ Comprehensive test coverage including **automated UI tests**
- ✅ Dependency injection for testability and maintainability
- ✅ RESTful API with Swagger documentation

## 🏗️ Architecture

### Backend (Web API)
The API follows a **clean, layered architecture** with clear separation of concerns:

- **Controllers Layer** (`ItemsController`)
  - Handles HTTP requests and responses
  - Maps between DTOs and domain models
  - Returns appropriate HTTP status codes (200, 201, 204, 400, 404)
  
- **Service Layer** (`ItemService`)
  - Contains business logic and validation
  - Input validation for names, descriptions, and categories
  - Orchestrates repository operations
  
- **Repository Layer** (`ItemRepository`)
  - Data access abstraction
  - In-memory storage with optional JSON file persistence
  - CRUD operations with search functionality
  
- **Models & DTOs**
  - Domain models for internal use
  - DTOs for API contracts (CreateItemDto, UpdateItemDto, ItemDto)
  
- **Features**
  - Dependency Injection via built-in DI container
  - RESTful API design with proper HTTP verbs
  - Swagger/OpenAPI documentation at `https://localhost:7044/swagger`
  - CORS enabled for local development

### Frontend (WPF Application)
The WPF app implements **MVVM pattern** with modern .NET practices:

- **Views** (`MainWindow.xaml`)
  - Pure XAML with no code-behind logic
  - Data binding to ViewModels
  - Material Design-inspired UI with custom color scheme
  
- **ViewModels** (`MainViewModel`)
  - Implements `INotifyPropertyChanged` for UI updates
  - `ObservableCollection<Item>` for automatic UI synchronization
  - Async command implementations (`AsyncRelayCommand`)
  - State management (loading, error messages, selected item)
  
- **Services**
  - `ApiService`: HTTP client wrapper for API communication
  - `CacheService`: Dual caching strategy (in-memory + file-based)
  - `NavigationService`: Future-ready navigation abstraction
  - `MockApiService`: For development without API dependency
  
- **Commands** (`RelayCommand`, `AsyncRelayCommand`)
  - Async command pattern for non-blocking operations
  - Proper error handling and UI feedback
  
- **Converters**
  - Boolean to Visibility converters
  - Null checking converters
  - Empty state converters for better UX
  
- **Features**
  - Dependency Injection configured in `App.xaml.cs`
  - Local caching for offline support
  - Cache stored at `%LocalAppData%\MiniDashboard\items_cache.json`
  - Responsive UI with loading indicators
  - Comprehensive error handling

### Caching Strategy (Bonus Feature)
The application implements a **dual-layer caching system**:

1. **In-Memory Cache** (`ConcurrentDictionary`)
   - Fast access for frequently used data
   - Thread-safe operations
   - Automatic UI updates via data binding

2. **File-Based Persistence**
   - JSON file storage at `%LocalAppData%\MiniDashboard\items_cache.json`
   - Survives application restarts
   - Automatic load on startup
   - Automatic save on data changes

3. **Cache Features**
   - Search cached items
   - Get items by ID
   - Track last update timestamp
   - Clear cache functionality
   - Fallback for offline mode

## 📁 Project Structure

```
MiniDashboard/
├── MiniDashboard.Api/                          # ASP.NET Core Web API
│   ├── Controllers/
│   │   └── ItemsController.cs                 # API endpoints
│   ├── Services/
│   │   ├── IItemService.cs                    # Service interface
│   │   └── ItemService.cs                     # Business logic
│   ├── Repositories/
│   │   ├── IItemRepository.cs                 # Repository interface
│   │   └── ItemRepository.cs                  # Data access
│   ├── Models/
│   │   └── Item.cs                            # Domain model
│   ├── DTOs/
│   │   ├── ItemDto.cs                         # Response DTO
│   │   ├── CreateItemDto.cs                   # Create request DTO
│   │   └── UpdateItemDto.cs                   # Update request DTO
│   ├── Program.cs                             # App configuration & DI
│   └── appsettings.json                       # Configuration
│
├── MiniDashboard.App/                          # WPF Desktop Application
│   ├── Models/
│   │   └── Item.cs                            # Client-side model
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs                   # Base ViewModel
│   │   └── MainViewModel.cs                   # Main window ViewModel
│   ├── Services/
│   │   ├── IApiService.cs                     # API service interface
│   │   ├── ApiService.cs                      # HTTP client wrapper
│   │   ├── MockApiService.cs                  # Mock for development
│   │   ├── ICacheService.cs                   # Cache interface
│   │   ├── CacheService.cs                    # Caching implementation
│   │   ├── INavigationService.cs              # Navigation interface
│   │   └── NavigationService.cs               # Navigation service
│   ├── Commands/
│   │   ├── RelayCommand.cs                    # Synchronous commands
│   │   └── AsyncRelayCommand.cs               # Async commands
│   ├── Converters/
│   │   ├── BoolToVisibilityConverter.cs       # UI converters
│   │   ├── NullToBoolConverter.cs
│   │   ├── StringToVisibilityConverter.cs
│   │   └── EmptyStateVisibilityConverter.cs
│   ├── Behaviors/
│   │   └── DataGridColumnHeaderClickBehavior.cs # Column sorting
│   ├── Resources/
│   │   └── ColorScheme.xaml                   # UI theme
│   ├── Images/
│   │   └── MachWorxLogo.png                   # Company logo
│   ├── MainWindow.xaml                        # Main UI
│   ├── MainWindow.xaml.cs                     # View code-behind
│   └── App.xaml.cs                            # App startup & DI
│
├── MiniDashboard.Tests/                        # Test Project
│   ├── ItemsControllerTests.cs                # Controller unit tests
│   ├── ItemServiceTests.cs                    # Service unit tests
│   ├── ItemRepositoryTests.cs                 # Repository unit tests
│   ├── MainViewModelTests.cs                  # ViewModel unit tests
│   ├── ApiServiceTests.cs                     # API service unit tests
│   ├── ItemsApiIntegrationTests.cs            # API integration tests
│   └── MainWindowUITests.cs                   # UI automation tests ⭐
│
├── MiniDashboard.IntegrationTests/             # Additional integration tests
│
├── README.md                                   # This file
├── QUICK_START.md                             # Quick start guide
├── SETUP_SUMMARY.md                           # Setup documentation
└── MiniDashboard.sln                          # Solution file
```

## 🚀 Getting Started

### Prerequisites

**Required:**
- .NET 8.0 SDK or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Visual Studio 2022 (recommended) or Visual Studio Code

**Optional:**
- Postman, Insomnia, or similar tool for API testing
- Git for version control

### Setup Instructions

#### Option 1: Using Visual Studio (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/machwork/MiniDashboard-RobinWarburton.git
   cd MiniDashboard-RobinWarburton
   ```

2. **Open the solution**
   - Open `MiniDashboard.sln` in Visual Studio 2022
   - Wait for NuGet packages to restore automatically

3. **Build the solution**
   - Press `Ctrl+Shift+B` or go to **Build → Build Solution**

4. **Run the API**
   - Right-click on `MiniDashboard.Api` project → **Set as Startup Project**
   - Press `F5` to run with debugging or `Ctrl+F5` to run without debugging
   - The API will launch at `https://localhost:7044`
   - Swagger UI: `https://localhost:7044/swagger`

5. **Run the WPF Application**
   - Right-click on `MiniDashboard.App` project → **Set as Startup Project**
   - Press `F5` to run
   - The application will connect to the API at `https://localhost:7044`

#### Option 2: Using Command Line

1. **Clone the repository**
   ```bash
   git clone https://github.com/machwork/MiniDashboard-RobinWarburton.git
   cd MiniDashboard-RobinWarburton
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API** (in first terminal)
   ```bash
   cd MiniDashboard.Api
   dotnet run
   ```
   The API will be available at:
   - **HTTPS**: `https://localhost:7044`
   - **Swagger UI**: `https://localhost:7044/swagger`

5. **Run the WPF Application** (in second terminal)
   ```bash
   cd MiniDashboard.App
   dotnet run
   ```
   The WPF app will connect to `https://localhost:7044` automatically.

### Configuration

#### API Port Configuration
The API is configured to run on port **7044**. This is set in:
- `MiniDashboard.Api/Properties/launchSettings.json`
- `MiniDashboard.App/App.xaml.cs` (client configuration)

To change the port, update both files accordingly.

#### Using Mock API (Development Mode)
If you want to run the WPF app without the API:

1. Set environment variable:
   ```bash
   set USE_MOCK_API=true
   ```
2. Run the WPF app - it will use `MockApiService` with sample data

#### Cache Location
The WPF app stores cached data at:
```
%LocalAppData%\MiniDashboard\items_cache.json
```
On Windows: `C:\Users\[YourUsername]\AppData\Local\MiniDashboard\items_cache.json`

## 🧪 Running Tests

This project includes comprehensive test coverage with **unit tests**, **integration tests**, and **UI automation tests**.

### Test Overview

The test suite includes:
- ✅ **22+ Unit Tests** - ViewModels, Services, Controllers, Repositories
- ✅ **8+ Integration Tests** - End-to-end API testing
- ✅ **6 UI Automation Tests** - Automated UI testing with TestStack.White

### Running All Tests

#### Using Visual Studio
1. Open **Test Explorer** (`Ctrl+E, T`)
2. Click **Run All Tests** button (green play icon)
3. View results in the Test Explorer window

#### Using Command Line
```bash
# Run all tests in the solution
dotnet test

# Run with detailed output
dotnet test -v detailed

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Running Specific Test Projects

**Run unit and integration tests only:**
```bash
dotnet test MiniDashboard.Tests
```

**Run additional integration tests:**
```bash
dotnet test MiniDashboard.IntegrationTests
```

### Running Specific Test Classes

```bash
# Run only controller tests
dotnet test --filter "FullyQualifiedName~ItemsControllerTests"

# Run only ViewModel tests
dotnet test --filter "FullyQualifiedName~MainViewModelTests"

# Run only service tests
dotnet test --filter "FullyQualifiedName~ItemServiceTests"

# Run only UI automation tests
dotnet test --filter "FullyQualifiedName~MainWindowUITests"
```

### Running Individual Tests

```bash
# Run a specific test method
dotnet test --filter "FullyQualifiedName~ItemsControllerTests.GetAllItems_ReturnsOkWithItems"
```

### UI Automation Tests (Bonus Feature ⭐)

The project includes **6 automated UI tests** using TestStack.White:

1. `MainWindow_ShouldLoad_WhenApplicationStarts`
2. `MainWindow_ShouldDisplayDataGrid_WhenLoaded`
3. `MainWindow_ShouldHaveAddNewItemButton_WhenLoaded`
4. `MainWindow_ShouldHaveSearchTextBox_WhenLoaded`
5. `MainWindow_ShouldHaveSearchButton_WhenLoaded`
6. `MainWindow_ShouldDisplayItemDetails_WhenItemIsSelected`

**Important Notes for UI Tests:**
- ⚠️ **Build the WPF app first** before running UI tests
- ⚠️ **Close any running instances** of MiniDashboard.App
- ⚠️ UI tests will **launch the actual WPF application** (windows will pop up)
- ⚠️ UI tests take longer than unit tests (2-10 seconds each)
- ⚠️ Don't interact with your computer while UI tests are running

**To run UI tests:**

```bash
# Build the WPF app first
dotnet build MiniDashboard.App

# Run UI automation tests
dotnet test --filter "FullyQualifiedName~MainWindowUITests"
```

### Test Results Interpretation

**Test Output:**
- ✅ **Passed** - Test executed successfully
- ❌ **Failed** - Test found an issue (check error message)
- ⚠️ **Skipped** - Test was not executed

**Example successful test run:**
```
Passed!  - Failed:     0, Passed:    36, Skipped:     0, Total:    36, Duration: 12 s
```

### Continuous Integration

These tests are designed to run in CI/CD pipelines. The UI automation tests require:
- Windows environment
- Display adapter (can use virtual display in CI)
- Built application artifacts

## 📡 API Endpoints

The API is available at `https://localhost:7044` and provides the following RESTful endpoints:

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/items` | Get all items | None | `200 OK` with `ItemDto[]` |
| GET | `/api/items/{id}` | Get item by ID | None | `200 OK` with `ItemDto` or `404 Not Found` |
| GET | `/api/items/search?query={text}` | Search items | None | `200 OK` with `ItemDto[]` |
| POST | `/api/items` | Create new item | `CreateItemDto` | `201 Created` with `ItemDto` |
| PUT | `/api/items/{id}` | Update existing item | `UpdateItemDto` | `200 OK` with `ItemDto` or `404 Not Found` |
| DELETE | `/api/items/{id}` | Delete item | None | `204 No Content` or `404 Not Found` |

### Data Models

**ItemDto (Response):**
```json
{
  "id": 1,
  "name": "Sample Item",
  "description": "Detailed description",
  "category": "Electronics",
  "createdAt": "2026-02-18T10:30:00Z",
  "lastModified": "2026-02-18T10:30:00Z"
}
```

**CreateItemDto (POST Request):**
```json
{
  "name": "New Item",           // Required, max 100 chars
  "description": "Description", // Required, max 500 chars
  "category": "Category"        // Required, max 50 chars
}
```

**UpdateItemDto (PUT Request):**
```json
{
  "name": "Updated Name",
  "description": "Updated description",
  "category": "Updated category"
}
```

### Example API Requests

#### Create Item
```http
POST https://localhost:7044/api/items
Content-Type: application/json

{
  "name": "Laptop",
  "description": "Dell XPS 15 with 16GB RAM",
  "category": "Electronics"
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /api/items/1
Content-Type: application/json

{
  "id": 1,
  "name": "Laptop",
  "description": "Dell XPS 15 with 16GB RAM",
  "category": "Electronics",
  "createdAt": "2026-02-18T10:30:00Z",
  "lastModified": "2026-02-18T10:30:00Z"
}
```

#### Get All Items
```http
GET https://localhost:7044/api/items
```

#### Search Items
```http
GET https://localhost:7044/api/items/search?query=laptop
```

#### Update Item
```http
PUT https://localhost:7044/api/items/1
Content-Type: application/json

{
  "name": "Gaming Laptop",
  "description": "Dell XPS 15 with RTX 3060",
  "category": "Gaming"
}
```

#### Delete Item
```http
DELETE https://localhost:7044/api/items/1
```

### Testing the API

**Using Swagger UI:**
1. Run the API
2. Navigate to `https://localhost:7044/swagger`
3. Expand an endpoint and click "Try it out"
4. Fill in parameters and click "Execute"

**Using Postman:**
1. Import the provided `MiniDashboard.Api.http` file
2. Update the base URL to `https://localhost:7044`
3. Execute requests

**Using curl:**
```bash
# Get all items
curl https://localhost:7044/api/items

# Create item
curl -X POST https://localhost:7044/api/items \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","description":"Test item","category":"Test"}'
```

## 🧪 Testing Strategy

This project implements a comprehensive 3-tier testing approach:

### 1. Unit Tests
**Purpose:** Test individual components in isolation

**Covered Components:**
- ✅ `ItemsControllerTests` - API controller logic
- ✅ `ItemServiceTests` - Business logic validation
- ✅ `ItemRepositoryTests` - Data access operations
- ✅ `MainViewModelTests` - WPF ViewModel behavior
- ✅ `ApiServiceTests` - HTTP client wrapper

**Approach:**
- Mock external dependencies using **Moq**
- Follow **AAA pattern** (Arrange, Act, Assert)
- Test both **positive and negative scenarios**
- Use **FluentAssertions** for readable assertions
- Async/await testing patterns

**Example Test:**
```csharp
[Fact]
public async Task AddItem_WithValidData_ShouldAddToCollection()
{
    // Arrange
    var mockApiService = new Mock<IApiService>();
    var viewModel = new MainViewModel(mockApiService.Object);
    
    // Act
    await viewModel.AddItemCommand.ExecuteAsync(null);
    
    // Assert
    viewModel.Items.Should().HaveCount(1);
}
```

### 2. Integration Tests
**Purpose:** Test API endpoints end-to-end with real HTTP requests

**Covered Scenarios:**
- ✅ `ItemsApiIntegrationTests` - Full CRUD operations via HTTP
- ✅ All API endpoints tested with various scenarios
- ✅ Validation error handling
- ✅ 404 Not Found scenarios

**Approach:**
- Use `Microsoft.AspNetCore.Mvc.Testing` for in-memory test server
- Real HTTP requests without mocking
- Verify **HTTP status codes** (200, 201, 204, 400, 404)
- Test **request/response serialization**
- Test **CRUD workflow** from start to finish

**Example Test:**
```csharp
[Fact]
public async Task CreateItem_ReturnsCreatedItem()
{
    // Arrange
    var client = _factory.CreateClient();
    var newItem = new CreateItemDto { Name = "Test", ... };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/items", newItem);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var item = await response.Content.ReadFromJsonAsync<ItemDto>();
    item.Name.Should().Be("Test");
}
```

### 3. UI Automation Tests (Bonus ⭐)
**Purpose:** Test the WPF application UI automatically

**Covered UI Elements:**
- ✅ Main window loads correctly
- ✅ DataGrid displays items
- ✅ Add button is present and enabled
- ✅ Search textbox is functional
- ✅ Search button works
- ✅ Item selection displays details

**Technology:**
- **TestStack.White** - UI automation framework
- **Windows UI Automation** - Microsoft's UI testing API
- Tests launch the **actual WPF application**
- Interact with controls by **AutomationId** and **text**

**Approach:**
- Launch application programmatically
- Wait for UI elements to load
- Interact with controls (click, type, select)
- Assert element states and visibility
- Clean up application after tests

**Example Test:**
```csharp
[Fact]
public void MainWindow_ShouldDisplayDataGrid_WhenLoaded()
{
    // Arrange
    StartApplication();
    WaitForApplicationToLoad();
    
    // Act
    var dataGrid = _window.Get<ListView>(
        SearchCriteria.ByAutomationId("ItemsDataGrid"));
    
    // Assert
    dataGrid.Should().NotBeNull();
}
```

### Test Coverage Summary

| Component | Unit Tests | Integration Tests | UI Tests | Total |
|-----------|-----------|-------------------|----------|-------|
| API Controllers | ✅ 5 | ✅ 6 | - | 11 |
| Services | ✅ 8 | - | - | 8 |
| Repositories | ✅ 5 | - | - | 5 |
| ViewModels | ✅ 6 | - | - | 6 |
| API Client | ✅ 4 | - | - | 4 |
| UI Components | - | - | ✅ 6 | 6 |
| **Total** | **28** | **6** | **6** | **40** |

## ✅ Implementation Status

### Backend (API) - Complete
- ✅ Create Item model/DTO (Item, ItemDto, CreateItemDto, UpdateItemDto)
- ✅ Implement IItemRepository interface
- ✅ Implement ItemRepository with in-memory storage
- ✅ Create IItemService interface
- ✅ Implement ItemService with business logic and validation
- ✅ Create ItemsController with full CRUD endpoints
- ✅ Add search functionality (`/api/items/search?query=`)
- ✅ Configure dependency injection in Program.cs
- ✅ Add proper error handling and HTTP status codes (200, 201, 204, 400, 404)
- ✅ Configure CORS for local development
- ✅ Add Swagger/OpenAPI documentation
- ✅ Write unit tests for service layer (8 tests)
- ✅ Write unit tests for controller layer (5 tests)
- ✅ Write unit tests for repository layer (5 tests)
- ✅ Write integration tests for all endpoints (6 tests)

### Frontend (WPF) - Complete
- ✅ Create Item model matching API DTO
- ✅ Create IApiService interface
- ✅ Implement ApiService with HttpClient
- ✅ Create MockApiService for development
- ✅ Create base ViewModel with INotifyPropertyChanged
- ✅ Create MainViewModel with ObservableCollection<Item>
- ✅ Implement ICommand (RelayCommand and AsyncRelayCommand)
- ✅ Implement all commands: Add, Edit, Delete, Search, Refresh, Clear Cache
- ✅ Create MainWindow.xaml with complete data binding
- ✅ Add Material Design-inspired UI with custom color scheme
- ✅ Add loading indicators with progress animation
- ✅ Add comprehensive error message display
- ✅ Add empty state handling
- ✅ Configure dependency injection in App.xaml.cs
- ✅ Write unit tests for ViewModels (6 tests)
- ✅ Write unit tests for ApiService (4 tests)

### Bonus Features - Complete ⭐
- ✅ **Local Caching System**
  - ✅ In-memory cache with ConcurrentDictionary
  - ✅ File-based persistence (JSON)
  - ✅ Cache at `%LocalAppData%\MiniDashboard\items_cache.json`
  - ✅ Automatic load on startup
  - ✅ Automatic save on changes
  - ✅ Search cached items
  - ✅ Offline mode support
  - ✅ Last update tracking

- ✅ **UI Automation Tests**
  - ✅ TestStack.White integration
  - ✅ 6 comprehensive UI tests
  - ✅ Window loading tests
  - ✅ Control presence tests
  - ✅ User interaction tests

### Documentation - Complete
- ✅ Comprehensive README.md with architecture
- ✅ Setup instructions for VS and CLI
- ✅ API endpoint documentation with examples
- ✅ Testing guide with multiple approaches
- ✅ Code examples and patterns
- ✅ Configuration documentation

## 📦 NuGet Packages

### MiniDashboard.Api
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- ASP.NET Core Web API (included in .NET 8.0 SDK)

### MiniDashboard.App
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.3" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.3" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="10.0.3" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.3" />
<PackageReference Include="System.Net.Http" Version="4.3.4" />
```
- **Microsoft.Extensions.DependencyInjection** - DI container
- **Microsoft.Extensions.Http** - IHttpClientFactory support
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **System.Net.Http** - HTTP client functionality

### MiniDashboard.Tests
```xml
<PackageReference Include="xunit" Version="2.5.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="TestStack.White" Version="0.13.3" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```
- **xUnit** - Testing framework
- **Moq** - Mocking library for unit tests
- **FluentAssertions** - Fluent assertion library
- **Microsoft.AspNetCore.Mvc.Testing** - In-memory API testing
- **TestStack.White** - UI automation framework
- **coverlet.collector** - Code coverage collector

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
The WPF app connects to the API at `https://localhost:7044` by default. This is configured in:

```csharp
// App.xaml.cs - ConfigureServices method
services.AddHttpClient<IApiService, ApiService>((provider, client) =>
{
    client.BaseAddress = new Uri("https://localhost:7044/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

**To change the API URL:**
1. Open `MiniDashboard.App/App.xaml.cs`
2. Update the `BaseAddress` in the `ConfigureServices` method
3. Rebuild the application

**Environment Variable Option:**
Set `USE_MOCK_API=true` to use mock data without the API:
```bash
set USE_MOCK_API=true
dotnet run --project MiniDashboard.App
```

## 🎨 Design Patterns & Architecture Principles

### SOLID Principles Compliance ✅

This project strictly adheres to all five SOLID principles of object-oriented design:

#### 1. **Single Responsibility Principle (SRP)** ✅
Each class has one, and only one, reason to change:

- **`ItemRepository`** - Responsible only for data access operations
- **`ItemService`** - Responsible only for business logic
- **`ItemsController`** - Responsible only for HTTP request/response handling
- **`MainViewModel`** - Responsible only for UI state management
- **`ApiService`** - Responsible only for HTTP communication
- **`CacheService`** - Responsible only for caching operations

#### 2. **Open/Closed Principle (OCP)** ✅
Classes are open for extension but closed for modification:

- Interface-based design allows new implementations without changing existing code
- Example: `MockApiService` extends functionality without modifying `ApiService`
- New repository implementations can be added without changing `ItemService`

```csharp
// Can add new implementations without changing existing code
public class SqlItemRepository : IItemRepository { }
public class CachedItemService : IItemService { }
```

#### 3. **Liskov Substitution Principle (LSP)** ✅
Derived classes/implementations are perfectly substitutable for their base types:

- `MockApiService` can completely replace `ApiService`
- `ItemRepository` can be replaced with any `IItemRepository` implementation
- All interface implementations maintain the contract expected by consumers

```csharp
// Both work identically from the ViewModel's perspective
IApiService apiService = new ApiService(httpClient, logger, cache);
IApiService mockService = new MockApiService();
```

#### 4. **Interface Segregation Principle (ISP)** ✅
Interfaces are small, focused, and client-specific:

- **`IItemService`** - Only 7 methods, all related to item business logic
- **`IApiService`** - Only 6 methods, all related to API communication
- **`ICacheService`** - Only 7 methods, all related to caching
- **`INavigationService`** - Only 1 method for navigation

No client is forced to depend on methods it doesn't use.

#### 5. **Dependency Inversion Principle (DIP)** ✅
High-level modules depend on abstractions, not concrete implementations:

```csharp
// API Layer - depends on interfaces, not implementations
public class ItemsController : ControllerBase {
    private readonly IItemService _itemService;
    public ItemsController(IItemService itemService) {
        _itemService = itemService;  // Injected via DI
    }
}

public class ItemService : IItemService {
    private readonly IItemRepository _repository;
    public ItemService(IItemRepository repository) {
        _repository = repository;  // Injected via DI
    }
}

// WPF Layer - depends on interfaces
public class MainViewModel : ViewModelBase {
    private readonly IApiService _apiService;
    public MainViewModel(IApiService apiService) {
        _apiService = apiService;  // Injected via DI
    }
}
```

**DI Configuration:**
```csharp
// API: Program.cs
builder.Services.AddSingleton<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

// WPF: App.xaml.cs
services.AddHttpClient<IApiService, ApiService>();
services.AddSingleton<ICacheService, CacheService>();
```

### Additional Design Patterns

- **Repository Pattern** - `ItemRepository` abstracts data access
- **Service Layer Pattern** - `ItemService` encapsulates business logic
- **MVVM Pattern** - Complete separation of UI and business logic
- **Command Pattern** - `RelayCommand` and `AsyncRelayCommand` for user actions
- **Factory Pattern** - `IHttpClientFactory` for creating HTTP clients
- **Observer Pattern** - `INotifyPropertyChanged` for data binding
- **Strategy Pattern** - Swappable implementations (`ApiService` vs `MockApiService`)
- **Facade Pattern** - Services provide simplified interfaces to complex subsystems

## 📝 Code Quality Standards

This project adheres to industry-standard coding practices:

### Architecture & Design ⭐
- ✅ **SOLID Principles** - Fully compliant with all five principles (see above)
- ✅ **Separation of Concerns** - Clear boundaries between layers
- ✅ **Dependency Injection** - Constructor injection throughout
- ✅ **Interface-based Design** - Program to interfaces, not implementations

### Code Practices
- ✅ **Async/Await** - All I/O operations use async/await for responsiveness
- ✅ **Error Handling** - Try-catch blocks with proper logging and user feedback
- ✅ **XML Documentation** - All public APIs documented with summary comments
- ✅ **Meaningful Naming** - Clear, descriptive names for variables, methods, and classes
- ✅ **Single Responsibility** - Each method has one clear purpose
- ✅ **DRY Principle** - No code duplication; shared logic extracted to methods
- ✅ **Null Safety** - Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- ✅ **Immutability** - DTOs and models designed for immutability where appropriate

### Testing
- ✅ **Comprehensive Coverage** - 40+ tests across unit, integration, and UI levels
- ✅ **AAA Pattern** - Arrange, Act, Assert in all tests
- ✅ **Mocking** - External dependencies mocked for unit testing
- ✅ **Test Isolation** - Each test is independent and can run in any order

## 🚀 Future Enhancements

Potential areas for expansion:

1. **Database Integration** - Replace in-memory storage with Entity Framework Core and SQL Server
2. **Authentication & Authorization** - Add user login and role-based access control
3. **Advanced Filtering** - Add category filters, date range filters, sorting options
4. **Export Functionality** - Export items to CSV, Excel, or PDF
5. **Real-time Updates** - Implement SignalR for real-time notifications
6. **Multi-language Support** - Add localization for international users
7. **Dark Mode** - Implement theme switching
8. **Advanced Caching** - Add cache expiration policies and Redis support
9. **Reporting** - Add charts, graphs, and analytics
10. **CI/CD Pipeline** - Automate build, test, and deployment with GitHub Actions

## 📄 License

This project is created for demonstration purposes as part of a technical assessment.

## 👤 Author

**Robin Warburton**
- Organization: MachWorx
- Repository: [MiniDashboard-RobinWarburton](https://github.com/machwork/MiniDashboard-RobinWarburton)

## 🙏 Acknowledgments

- **TestStack.White** - For UI automation capabilities
- **xUnit** - For the testing framework
- **Moq** - For mocking in unit tests
- **FluentAssertions** - For readable test assertions
- **Swashbuckle** - For API documentation

---



