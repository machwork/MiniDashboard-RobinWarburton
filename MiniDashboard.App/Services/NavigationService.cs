using System.Diagnostics;

namespace MiniDashboard.App.Services;

/// <summary>
/// Implementation of INavigationService for opening URLs
/// </summary>
public class NavigationService : INavigationService
{
    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to open URL: {url}", ex);
        }
    }
}

