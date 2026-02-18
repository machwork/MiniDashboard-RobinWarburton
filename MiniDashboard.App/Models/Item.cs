using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MiniDashboard.App.Models;

/// <summary>
/// Item model for WPF application
/// </summary>
public class Item : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _category = string.Empty;
    private decimal _price;
    private DateTime _createdDate;
    private DateTime? _updatedDate;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetProperty(ref _createdDate, value);
    }

    public DateTime? UpdatedDate
    {
        get => _updatedDate;
        set => SetProperty(ref _updatedDate, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}



