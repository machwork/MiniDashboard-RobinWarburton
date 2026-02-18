using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MiniDashboard.App.ViewModels;

namespace MiniDashboard.App.Behaviors;

/// <summary>
/// Behavior to handle DataGrid column header clicks and execute a sort command
/// </summary>
public class DataGridColumnHeaderClickBehavior
{
    public static readonly DependencyProperty SortCommandProperty =
        DependencyProperty.RegisterAttached(
            "SortCommand",
            typeof(ICommand),
            typeof(DataGridColumnHeaderClickBehavior),
            new PropertyMetadata(null, OnSortCommandChanged));

    public static ICommand GetSortCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(SortCommandProperty);
    }

    public static void SetSortCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(SortCommandProperty, value);
    }

    private static void OnSortCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid)
        {
            if (e.NewValue != null)
            {
                dataGrid.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));
            }
            else
            {
                dataGrid.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));
            }
        }
    }

    private static void OnColumnHeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is DataGridColumnHeader header && sender is DataGrid dataGrid)
        {
            var sortCommand = GetSortCommand(dataGrid);
            if (sortCommand != null)
            {
                string? columnName = null;

                // Try to get column name from header content
                if (header.Content is string content)
                {
                    columnName = content;
                }
                else if (header.Content != null)
                {
                    columnName = header.Content.ToString();
                }

                // If we still don't have a column name, try to get it from the column
                if (string.IsNullOrEmpty(columnName) && header.Column is DataGridTextColumn textColumn)
                {
                    // Map header text to property name
                    columnName = textColumn.Header?.ToString() switch
                    {
                        "ID" => "Id",
                        "Name" => "Name",
                        "Description" => "Description",
                        "Category" => "Category",
                        "Price" => "Price",
                        "Created" => "CreatedDate",
                        _ => null
                    };
                }

                if (!string.IsNullOrEmpty(columnName) && sortCommand.CanExecute(columnName))
                {
                    sortCommand.Execute(columnName);
                }
            }
        }
    }
}

