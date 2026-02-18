using FluentAssertions;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.WindowItems;
using Xunit;

namespace MiniDashboard.Tests;

public class MainWindowUITests : IDisposable
{
    private Application? _application;
    private Window? _window;
    private readonly string _appPath;

    public MainWindowUITests()
    {
        // Get the path to the compiled application
        var appDirectory = Path.GetDirectoryName(typeof(MainWindowUITests).Assembly.Location);
        var solutionDirectory = Directory.GetParent(appDirectory!)!.Parent!.Parent!.Parent!.FullName;
        _appPath = Path.Combine(solutionDirectory, "MiniDashboard.App", "bin", "Debug", "net8.0-windows", "MiniDashboard.App.exe");
    }

    [Fact]
    public void MainWindow_ShouldLoad_WhenApplicationStarts()
    {
        // Arrange & Act
        StartApplication();

        // Assert
        _window.Should().NotBeNull();
        _window!.Title.Should().Be("Mini Dashboard");
    }

    [Fact]
    public void MainWindow_ShouldDisplayDataGrid_WhenLoaded()
    {
        // Arrange
        StartApplication();
        WaitForApplicationToLoad();

        // Act
        var dataGrid = _window!.Get<ListView>(SearchCriteria.ByAutomationId("ItemsDataGrid"));

        // Assert
        dataGrid.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveAddNewItemButton_WhenLoaded()
    {
        // Arrange
        StartApplication();
        WaitForApplicationToLoad();

        // Act
        var addButton = _window!.Get<Button>(SearchCriteria.ByText("Add New Item"));

        // Assert
        addButton.Should().NotBeNull();
        addButton.Enabled.Should().BeTrue();
    }

    [Fact]
    public void MainWindow_ShouldHaveSearchTextBox_WhenLoaded()
    {
        // Arrange
        StartApplication();
        WaitForApplicationToLoad();

        // Act
        var searchTextBox = _window!.Get<TextBox>(SearchCriteria.ByAutomationId("SearchTextBox"));

        // Assert
        searchTextBox.Should().NotBeNull();
    }

    [Fact]
    public void MainWindow_ShouldHaveSearchButton_WhenLoaded()
    {
        // Arrange
        StartApplication();
        WaitForApplicationToLoad();

        // Act
        var searchButton = _window!.Get<Button>(SearchCriteria.ByText("Search"));

        // Assert
        searchButton.Should().NotBeNull();
        searchButton.Enabled.Should().BeTrue();
    }

    [Fact]
    public void MainWindow_ShouldDisplayItemDetails_WhenItemIsSelected()
    {
        // Arrange
        StartApplication();
        WaitForApplicationToLoad();

        // Act - Try to find and select an item in the grid
        var dataGrid = _window!.Get<ListView>(SearchCriteria.ByAutomationId("ItemsDataGrid"));
        
        if (dataGrid.Items.Count > 0)
        {
            // Select the first item in the ListView
            dataGrid.Select(0);
            
            // Wait a bit for selection to update
            System.Threading.Thread.Sleep(500);
            
            // Try to find item detail fields
            var nameTextBox = _window.Get<TextBox>(SearchCriteria.ByAutomationId("NameTextBox"));
            
            // Assert - If we can find the name textbox, item details are displayed
            nameTextBox.Should().NotBeNull();
        }
    }

    private void StartApplication()
    {
        if (!File.Exists(_appPath))
        {
            throw new FileNotFoundException($"Application not found at: {_appPath}");
        }

        var processStartInfo = new ProcessStartInfo(_appPath)
        {
            UseShellExecute = false
        };

        _application = Application.Launch(processStartInfo);
        _window = _application.GetWindow("Mini Dashboard", InitializeOption.NoCache);
    }

    private void WaitForApplicationToLoad()
    {
        // Wait for the application to fully load
        System.Threading.Thread.Sleep(2000);
        
        // Wait for any loading indicators to disappear
        var maxWaitTime = TimeSpan.FromSeconds(10);
        var startTime = DateTime.Now;
        
        while (DateTime.Now - startTime < maxWaitTime)
        {
            try
            {
                // Try to find a control that should be visible when loaded
                var dataGrid = _window!.Get<ListView>(SearchCriteria.ByAutomationId("ItemsDataGrid"));
                if (dataGrid != null)
                {
                    break;
                }
            }
            catch
            {
                // Control not found yet, wait a bit more
                System.Threading.Thread.Sleep(500);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _application?.Close();
            _application?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}

