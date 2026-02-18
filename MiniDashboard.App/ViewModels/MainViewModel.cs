using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using MiniDashboard.App.Commands;
using MiniDashboard.App.Models;
using MiniDashboard.App.Services;
using System;

namespace MiniDashboard.App.ViewModels;

/// <summary>
/// Main ViewModel for the dashboard
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly INavigationService? _navigationService;
    private ObservableCollection<Item> _items = new(); // Always initialized, never null
    private ObservableCollection<Item> _allItems = new(); // Store all items for pagination
    private ObservableCollection<Item> _originalItems = new(); // Store original unfiltered items
    private Item? _selectedItem;
    private string _searchQuery = string.Empty;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private string _sortColumn = "Name";
    private bool _sortAscending = true;
    private string _customSortValue = string.Empty;
    private string? _customSortColumn = null;
    private int _currentPage = 1;
    private const int _pageSize = 50;
    private int _totalPages = 1;

    public MainViewModel(IApiService apiService, INavigationService? navigationService = null)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _navigationService = navigationService;
        
        // Initialize available columns for custom sort
        AvailableSortColumns = new List<string> { "ID", "Name", "Description", "Category", "Price", "Created" };
        
        // Subscribe to collection changes
        _items.CollectionChanged += Items_CollectionChanged;
        
        // Use AsyncRelayCommand for async operations so they are properly awaited and prevent re-entrancy
        LoadItemsCommand = new AsyncRelayCommand(async _ => await LoadItemsAsync(), _ => !IsLoading);
        AddItemCommand = new RelayCommand(_ => AddNewItem(), _ => !IsLoading);
        EditItemCommand = new RelayCommand(_ => EditSelectedItem(), _ => SelectedItem != null && !IsLoading);
        DeleteItemCommand = new AsyncRelayCommand(async _ => await DeleteSelectedItemAsync(), _ => SelectedItem != null && !IsLoading);
        SearchCommand = new AsyncRelayCommand(async _ => await SearchItemsAsync(), _ => !IsLoading);
        SortCommand = new RelayCommand(p => SortItems(p?.ToString() ?? "Name"), _ => !IsLoading);
        ApplyCustomSortCommand = new RelayCommand(_ => ApplyCustomSort(), _ => !IsLoading && !string.IsNullOrWhiteSpace(CustomSortValue) && IsCustomSortColumnSelected);
        ClearCustomSortCommand = new RelayCommand(_ => ClearCustomSort(), _ => !IsLoading);
        SaveItemCommand = new AsyncRelayCommand(async _ => await SaveItemAsync(), _ => SelectedItem != null && !IsLoading);
        CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => !IsLoading);
        ClearErrorCommand = new RelayCommand(_ => ClearError(), _ => !string.IsNullOrWhiteSpace(ErrorMessage));
        NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CurrentPage < TotalPages && !IsLoading);
        PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CurrentPage > 1 && !IsLoading);
        FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => CurrentPage > 1 && !IsLoading);
        LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => CurrentPage < TotalPages && !IsLoading);
        OpenMachWorxWebsiteCommand = new RelayCommand(_ => OpenMachWorxWebsite(), _ => _navigationService != null);
        
        // Initialize data loading
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Startup load failed: {ex.Message}";
        }
    }

    private void OpenMachWorxWebsite()
    {
        if (_navigationService != null)
        {
            try
            {
                _navigationService.OpenUrl("https://www.machworx.co.nz/index.html");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unable to open website: {ex.Message}";
            }
        }
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set
        {
            if (_items != null)
            {
                _items.CollectionChanged -= Items_CollectionChanged;
            }
            
            // Ensure value is never null - use empty collection if null is passed
            var itemsValue = value ?? new ObservableCollection<Item>();
            
            // SetProperty will assign itemsValue to _items, and itemsValue is guaranteed non-null
#pragma warning disable CS8601 // itemsValue is guaranteed non-null due to null-coalescing above
            if (SetProperty(ref _items, itemsValue))
#pragma warning restore CS8601
            {
                // _items is guaranteed to be non-null here since itemsValue was non-null
#pragma warning disable CS8602 // _items is guaranteed non-null since itemsValue was non-null
                _items.CollectionChanged += Items_CollectionChanged;
#pragma warning restore CS8602
                OnPropertyChanged(nameof(ItemsCount));
            }
        }
    }

    public int ItemsCount => _allItems?.Count ?? 0;

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                CommandManager.InvalidateRequerySuggested();
                ApplyPagination();
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (SetProperty(ref _totalPages, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string PageInfo => TotalPages > 1 
        ? $"Page {CurrentPage} of {TotalPages} (Showing {Items.Count} of {_allItems.Count} items)"
        : $"Showing {Items.Count} items";

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ItemsCount));
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            SetProperty(ref _isLoading, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string SortColumn
    {
        get => _sortColumn;
        set => SetProperty(ref _sortColumn, value);
    }

    public bool SortAscending
    {
        get => _sortAscending;
        set => SetProperty(ref _sortAscending, value);
    }

    public string CustomSortValue
    {
        get => _customSortValue;
        set
        {
            if (SetProperty(ref _customSortValue, value))
            {
                // Apply custom sort when value changes (only if column is selected)
                if (!string.IsNullOrWhiteSpace(_customSortValue) && !string.IsNullOrWhiteSpace(CustomSortColumn) && _allItems != null && _allItems.Count > 0)
                {
                    ApplyCustomSort();
                }
                else if (string.IsNullOrWhiteSpace(_customSortValue) && _allItems != null && _allItems.Count > 0)
                {
                    // Reset to normal sort when custom value is cleared
                    SortItems(SortColumn);
                }
            }
        }
    }

    public string? CustomSortColumn
    {
        get => _customSortColumn;
        set
        {
            if (SetProperty(ref _customSortColumn, value))
            {
                OnPropertyChanged(nameof(IsCustomSortColumnSelected));
                // Clear custom sort value when column is deselected
                if (string.IsNullOrWhiteSpace(_customSortColumn))
                {
                    CustomSortValue = string.Empty;
                }
                // Re-apply custom sort when column changes
                else if (!string.IsNullOrWhiteSpace(CustomSortValue) && _allItems != null && _allItems.Count > 0)
                {
                    ApplyCustomSort();
                }
            }
        }
    }

    public bool IsCustomSortColumnSelected => !string.IsNullOrWhiteSpace(_customSortColumn);

    public List<string> AvailableSortColumns { get; }

    public ICommand LoadItemsCommand { get; }
    public ICommand AddItemCommand { get; }
    public ICommand EditItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SortCommand { get; }
    public ICommand ApplyCustomSortCommand { get; }
    public ICommand ClearCustomSortCommand { get; }
    public ICommand SaveItemCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ClearErrorCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand OpenMachWorxWebsiteCommand { get; }

    // Make this public so callers (MainWindow) can await it and observe exceptions
    public async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var items = await _apiService.GetItemsAsync();
            _originalItems = new ObservableCollection<Item>(items);
            _allItems = new ObservableCollection<Item>(items);
            SortItems(SortColumn);
            ApplyPagination();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading items: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddNewItem()
    {
        SelectedItem = new Item
        {
            Name = string.Empty,
            Description = string.Empty,
            Category = string.Empty,
            Price = 0
        };
    }

    private void EditSelectedItem()
    {
        // Item is already selected, just need to ensure it's editable
        if (SelectedItem != null)
        {
            // Create a copy for editing
            SelectedItem = new Item
            {
                Id = SelectedItem.Id,
                Name = SelectedItem.Name,
                Description = SelectedItem.Description,
                Category = SelectedItem.Category,
                Price = SelectedItem.Price,
                CreatedDate = SelectedItem.CreatedDate,
                UpdatedDate = SelectedItem.UpdatedDate
            };
        }
    }

    private async Task DeleteSelectedItemAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var deleted = await _apiService.DeleteItemAsync(SelectedItem.Id);
            if (deleted)
            {
                _allItems.Remove(SelectedItem);
                SelectedItem = null;
                ApplyPagination();
            }
            else
            {
                ErrorMessage = "Failed to delete item";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchItemsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            IEnumerable<Item> items;
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                items = await _apiService.GetItemsAsync();
                _originalItems = new ObservableCollection<Item>(items);
            }
            else
            {
                items = await _apiService.SearchItemsAsync(SearchQuery);
                // Keep original items if we already have them, otherwise load them
                if (_originalItems.Count == 0)
                {
                    var allItems = await _apiService.GetItemsAsync();
                    _originalItems = new ObservableCollection<Item>(allItems);
                }
            }

            _allItems = new ObservableCollection<Item>(items);
            SortItems(SortColumn);
            ApplyPagination();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error searching items: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SortItems(string? columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName)) return;

        if (_allItems == null || _allItems.Count == 0) return;

        var items = _allItems.ToList();
        
        if (SortColumn == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortColumn = columnName;
            SortAscending = true;
        }

        items = SortColumn switch
        {
            "Id" or "ID" => SortAscending 
                ? items.OrderBy(i => i.Id).ToList()
                : items.OrderByDescending(i => i.Id).ToList(),
            "Name" => SortAscending 
                ? items.OrderBy(i => i.Name).ToList()
                : items.OrderByDescending(i => i.Name).ToList(),
            "Description" => SortAscending
                ? items.OrderBy(i => i.Description).ToList()
                : items.OrderByDescending(i => i.Description).ToList(),
            "Category" => SortAscending
                ? items.OrderBy(i => i.Category).ToList()
                : items.OrderByDescending(i => i.Category).ToList(),
            "Price" => SortAscending
                ? items.OrderBy(i => i.Price).ToList()
                : items.OrderByDescending(i => i.Price).ToList(),
            "CreatedDate" or "Created" => SortAscending
                ? items.OrderBy(i => i.CreatedDate).ToList()
                : items.OrderByDescending(i => i.CreatedDate).ToList(),
            _ => items
        };

        // Apply custom sort if value is provided
        if (!string.IsNullOrWhiteSpace(CustomSortValue))
        {
            items = ApplyCustomSortLogic(items);
        }

        _allItems = new ObservableCollection<Item>(items);
        CurrentPage = 1; // Reset to first page when sorting
        ApplyPagination();
    }

    private void ApplyCustomSort()
    {
        if (string.IsNullOrWhiteSpace(CustomSortValue) || string.IsNullOrWhiteSpace(CustomSortColumn)) return;
        
        // If no items loaded, return (user should load items first)
        if (_allItems == null || _allItems.Count == 0) return;

        // Work with the current _allItems (which could be search results or all items)
        // This ensures custom sort works on whatever is currently displayed
        var itemsToSort = _allItems.ToList();
        
        // Apply custom sort logic ONLY on the selected column (this may filter/sort items)
        var sortedItems = ApplyCustomSortLogic(itemsToSort);
        
        // Update _allItems with the sorted/filtered results
        _allItems = new ObservableCollection<Item>(sortedItems);
        CurrentPage = 1;
        ApplyPagination();
        
        // Notify UI that items have changed
        OnPropertyChanged(nameof(ItemsCount));
        OnPropertyChanged(nameof(PageInfo));
    }

    private void ClearCustomSort()
    {
        // Clear the column first to avoid triggering value clear in the setter
        _customSortColumn = null;
        OnPropertyChanged(nameof(CustomSortColumn));
        OnPropertyChanged(nameof(IsCustomSortColumnSelected));
        
        // Clear the value
        _customSortValue = string.Empty;
        OnPropertyChanged(nameof(CustomSortValue));
        
        // Reload items to show the original unfiltered list
        if (_allItems != null && _allItems.Count > 0)
        {
            // Reload from API to get original items
            _ = LoadItemsAsync();
        }
    }

    private List<Item> ApplyCustomSortLogic(List<Item> items)
    {
        if (string.IsNullOrWhiteSpace(CustomSortValue) || string.IsNullOrWhiteSpace(CustomSortColumn)) return items;

        var customValue = CustomSortValue.Trim();
        var sortColumn = CustomSortColumn;
        
        // Sort based ONLY on the selected column
        if (string.Equals(sortColumn, "ID", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(customValue, out int targetId))
            {
                // Filter to only exact ID matches
                var filtered = items.Where(item => item.Id == targetId).ToList();
                return filtered;
            }
            // If not a number, filter to IDs that contain the text
            return items.Where(item => item.Id.ToString().Contains(customValue, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        return sortColumn switch
        {
            
            "Name" => items.OrderBy(item =>
            {
                var lowerValue = customValue.ToLowerInvariant();
                var itemName = item.Name.ToLowerInvariant();
                // Prioritize items that start with the value, then contain it
                if (itemName.StartsWith(lowerValue)) return 0;
                if (itemName.Contains(lowerValue)) return 1;
                return 2;
            }).ThenBy(item => item.Name).ToList(),
            
            "Description" => items.OrderBy(item =>
            {
                var lowerValue = customValue.ToLowerInvariant();
                var itemDesc = item.Description.ToLowerInvariant();
                if (itemDesc.StartsWith(lowerValue)) return 0;
                if (itemDesc.Contains(lowerValue)) return 1;
                return 2;
            }).ThenBy(item => item.Description).ToList(),
            
            "Category" => items.OrderBy(item =>
            {
                var lowerValue = customValue.ToLowerInvariant();
                var itemCat = item.Category.ToLowerInvariant();
                if (itemCat.StartsWith(lowerValue)) return 0;
                if (itemCat.Contains(lowerValue)) return 1;
                return 2;
            }).ThenBy(item => item.Category).ToList(),
            
            "Price" => items.OrderBy(item =>
            {
                if (decimal.TryParse(customValue, out decimal targetPrice))
                {
                    // Sort by distance from target price
                    return Math.Abs((double)(item.Price - targetPrice));
                }
                // If not a number, check if price contains the text
                return item.Price.ToString().Contains(customValue) ? 0 : double.MaxValue;
            }).ToList(),
            
            "Created" or "CreatedDate" => items.OrderBy(item =>
            {
                if (DateTime.TryParse(customValue, out DateTime targetDate))
                {
                    // Sort by distance from target date
                    return Math.Abs((item.CreatedDate - targetDate).TotalDays);
                }
                // If not a date, check if date string contains the text
                return item.CreatedDate.ToString().Contains(customValue) ? 0 : double.MaxValue;
            }).ToList(),
            
            _ => items
        };
    }

    private void ApplyPagination()
    {
        if (_allItems == null || _allItems.Count == 0)
        {
            Items = new ObservableCollection<Item>();
            TotalPages = 1;
            CurrentPage = 1;
            OnPropertyChanged(nameof(PageInfo));
            return;
        }

        // Calculate total pages
        TotalPages = (int)Math.Ceiling((double)_allItems.Count / _pageSize);
        
        // Ensure current page is valid
        if (CurrentPage > TotalPages)
        {
            CurrentPage = TotalPages;
        }
        if (CurrentPage < 1)
        {
            CurrentPage = 1;
        }

        // Get items for current page
        var startIndex = (CurrentPage - 1) * _pageSize;
        var pageItems = _allItems.Skip(startIndex).Take(_pageSize).ToList();
        
        Items = new ObservableCollection<Item>(pageItems);
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(ItemsCount));
    }

    private void GoToNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private void GoToPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void GoToFirstPage()
    {
        CurrentPage = 1;
    }

    private void GoToLastPage()
    {
        CurrentPage = TotalPages;
    }

    private async Task SaveItemAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Item savedItem;
            if (SelectedItem.Id == 0)
            {
                // Create new item
                savedItem = await _apiService.CreateItemAsync(SelectedItem);
                _allItems.Add(savedItem);
            }
            else
            {
                // Update existing item
                var updatedItem = await _apiService.UpdateItemAsync(SelectedItem.Id, SelectedItem);
                if (updatedItem != null)
                {
                    savedItem = updatedItem;
                    var index = _allItems.IndexOf(_allItems.FirstOrDefault(i => i.Id == SelectedItem.Id)!);
                    if (index >= 0)
                    {
                        _allItems[index] = savedItem;
                    }
                }
                else
                {
                    ErrorMessage = "Error: Item could not be updated.";
                    return;
                }
            }

            // Clear selection after successful save
            SelectedItem = null;
            SortItems(SortColumn);
            ApplyPagination();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelEdit()
    {
        SelectedItem = null;
        ErrorMessage = string.Empty;
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
    }
}


