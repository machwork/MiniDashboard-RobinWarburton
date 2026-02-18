using System.Windows;
using MiniDashboard.App.ViewModels;

namespace MiniDashboard.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        // Data loading is now handled in ViewModel initialization - no code-behind needed
    }


}