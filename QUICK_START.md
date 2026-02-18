# Quick Start Commands

## Build and Run

### Build the entire solution
```bash
dotnet build
```

### Run the API
```bash
cd MiniDashboard.Api
dotnet run
```
Then open: `https://localhost:5001/swagger`

### Run the WPF Application
```bash
cd MiniDashboard.App
dotnet run
```

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test MiniDashboard.Tests
dotnet test MiniDashboard.IntegrationTests
```

## Development Workflow

1. **Start the API first** (in one terminal):
   ```bash
   cd MiniDashboard.Api
   dotnet watch run
   ```

2. **Run the WPF app** (in another terminal):
   ```bash
   cd MiniDashboard.App
   dotnet run
   ```

3. **Run tests** (in another terminal or when needed):
   ```bash
   dotnet test --watch
   ```

## Clean Build

To clean and rebuild:
```bash
dotnet clean
dotnet build
```

## Restore Packages

If you need to restore packages:
```bash
dotnet restore
```



