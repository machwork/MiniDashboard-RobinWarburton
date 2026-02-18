namespace MiniDashboard.App.Services;

/// <summary>
/// Service for opening URLs in the default browser
/// </summary>
public interface INavigationService
{
    void OpenUrl(string url);
}

