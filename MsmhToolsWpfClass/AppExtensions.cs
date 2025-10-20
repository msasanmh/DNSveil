using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MsmhToolsClass;
using Brush = System.Windows.Media.Brush;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace MsmhToolsWpfClass;

public static class AppExtensions
{
    public static void DispatchIt(this UIElement element, Action action)
    {
        try
        {
            // If Dispatch Is Not Required
            if (element.Dispatcher.CheckAccess()) action();
            else element.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, action);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions DispatchIt (UIElement): " + ex.Message);
        }
    }

    public static void DispatchIt(this Application app, Action action)
    {
        try
        {
            // If Dispatch Is Not Required
            if (app.Dispatcher.CheckAccess()) action();
            else app.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, action);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions DispatchIt (Application): " + ex.Message);
        }
    }

    public static async Task DispatchItAsync(this UIElement element, Action action) // TEST
    {
        try
        {
            // If Dispatch Is Not Required
            if (element.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await element.Dispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions DispatchItAsync (UIElement): " + ex.Message);
        }
    }

    public static async void DispatchItAsync(this Application app, Action action) // TEST
    {
        try
        {
            // If Dispatch Is Not Required
            if (app.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await app.Dispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions DispatchItAsync (Application): " + ex.Message);
        }
    }

    public static bool IsWindowOpen<T>(this Window? window) where T : Window
    {
        bool result = false;
        
        try
        {
            if (window == null) return result;
            window.DispatchIt(() =>
            {
                result = string.IsNullOrEmpty(window.Name)
                       ? Application.Current.Windows.OfType<T>().Any(x => x.GetHashCode().Equals(window.GetHashCode()))
                       : Application.Current.Windows.OfType<T>().Any(x => x.Name.Equals(window.Name));
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions IsWindowOpen: " + ex.Message);
        }

        return result;
    }

    public static void SetText(this TextBlock t, string text)
    {
        t.DispatchIt(() => t.Text = text);
    }

    public static void Clear(this TextBlock textBlock)
    {
        textBlock.DispatchIt(() =>
        {
            textBlock.Inlines.Clear();
            textBlock.Text = string.Empty;
        });
    }

    public static void AppendNewLine(this TextBlock t, bool twoNewLines = false)
    {
        t.DispatchIt(() =>
        {
            t.Inlines.Add(new LineBreak());
            if (twoNewLines) t.Inlines.Add(new LineBreak());
        });
    }

    public static void AppendText(this TextBlock t, string text, Brush? brush = null)
    {
        t.DispatchIt(() => t.Inlines.Add(new Run(text) { Foreground = brush ?? t.Foreground }));
    }

    public static void AppendText(this TextBlock t, string text1, Brush? brush1, string text2, Brush? brush2)
    {
        t.DispatchIt(() =>
        {
            t.Inlines.Add(new Run(text1) { Foreground = brush1 ?? t.Foreground });
            t.Inlines.Add(new Run(text2) { Foreground = brush2 ?? t.Foreground });
        });
    }

    public static void AppendText(this TextBlock t, string text1, Brush? brush1, string text2, Brush? brush2, string text3, Brush? brush3)
    {
        t.DispatchIt(() =>
        {
            t.Inlines.Add(new Run(text1) { Foreground = brush1 ?? t.Foreground });
            t.Inlines.Add(new Run(text2) { Foreground = brush2 ?? t.Foreground });
            t.Inlines.Add(new Run(text3) { Foreground = brush3 ?? t.Foreground });
        });
    }

    public static void AppendText(this TextBlock t, string text1, Brush? brush1, string text2, Brush? brush2, string text3, Brush? brush3, string text4, Brush? brush4)
    {
        t.DispatchIt(() =>
        {
            t.Inlines.Add(new Run(text1) { Foreground = brush1 ?? t.Foreground });
            t.Inlines.Add(new Run(text2) { Foreground = brush2 ?? t.Foreground });
            t.Inlines.Add(new Run(text3) { Foreground = brush3 ?? t.Foreground });
            t.Inlines.Add(new Run(text4) { Foreground = brush4 ?? t.Foreground });
        });
    }

    public static void AppendText(this TextBlock t, string text1, Brush? brush1, string text2, Brush? brush2, string text3, Brush? brush3, string text4, Brush? brush4, string text5, Brush? brush5)
    {
        t.DispatchIt(() =>
        {
            t.Inlines.Add(new Run(text1) { Foreground = brush1 ?? t.Foreground });
            t.Inlines.Add(new Run(text2) { Foreground = brush2 ?? t.Foreground });
            t.Inlines.Add(new Run(text3) { Foreground = brush3 ?? t.Foreground });
            t.Inlines.Add(new Run(text4) { Foreground = brush4 ?? t.Foreground });
            t.Inlines.Add(new Run(text5) { Foreground = brush5 ?? t.Foreground });
        });
    }

    /// <summary>
    /// Disposable
    /// </summary>
    /// <returns>Application Icon</returns>
    public static Icon GetApplicationIcon(this Visual _)
    {
        if (!string.IsNullOrEmpty(Environment.ProcessPath))
            return Icon.ExtractAssociatedIcon(Path.GetFullPath(Environment.ProcessPath)) ?? SystemIcons.Application;
        return SystemIcons.Application;
    }

    public static BitmapSource ToImageSource(this Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

    public static DrawingImage ToImageSource(this Geometry geometry, Brush brush, Brush strokeBrush, double strokeThickness)
    {
        try
        {
            GeometryDrawing geometryDrawing = new(brush, new Pen(strokeBrush, strokeThickness), geometry);
            return new DrawingImage(geometryDrawing);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions ToImageSource (Geometry): " + ex.Message);
            return new DrawingImage();
        }
    }

    public static DrawingImage ToImageSource(this Geometry geometry, Brush brush)
    {
        return geometry.ToImageSource(brush, brush, 0);
    }

    public static Window? GetParentWindow(this DependencyObject child)
    {
        try
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is Window parentWindow) return parentWindow;
            else return GetParentWindow(parent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetParentWindow: " + ex.Message);
            return null;
        }
    }

    public static T? GetParentOfType<T>(this DependencyObject child)
    {
        try
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return default;
            if (parent is T parentT) return parentT;
            else return GetParentOfType<T>(parent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetParentOfType: " + ex.Message);
            return default;
        }
    }

    public static T? GetChildOfType<T>(this DependencyObject? depObj) where T : DependencyObject
    {
        try
        {
            if (depObj != null)
            {
                for (int n = 0; n < VisualTreeHelper.GetChildrenCount(depObj); n++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, n);
                    if (child is not null and T tChild) return tChild;
                    T? childOfChild = GetChildOfType<T>(child);
                    if (childOfChild != null) return childOfChild;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetChildOfType: " + ex.Message);
            return null;
        }
    }

    public static IEnumerable<T> GetChildrenOfType<T>(this DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int n = 0; n < VisualTreeHelper.GetChildrenCount(depObj); n++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, n);
                if (child is not null and T tChild) yield return tChild;
                foreach (T childOfChild in GetChildrenOfType<T>(child)) yield return childOfChild;
            }
        }
    }

    private static Geometry GetClippedRectangle(double radiusX, double radiusY, object bindingSource)
    {
        try
        {
            // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!
            MultiBinding multiBinding = new();
            multiBinding.Converter = new SizeToRectMultiConverter();
            multiBinding.Bindings.Add(new Binding("ActualWidth") { Source = bindingSource });
            multiBinding.Bindings.Add(new Binding("ActualHeight") { Source = bindingSource });
            multiBinding.NotifyOnSourceUpdated = true;

            RectangleGeometry rectangleGeometry = new()
            {
                RadiusX = radiusX,
                RadiusY = radiusY
            };
            BindingOperations.SetBinding(rectangleGeometry, RectangleGeometry.RectProperty, multiBinding);
            return rectangleGeometry;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetClippedRectangle: " + ex.Message);
            return Geometry.Empty;
        }
    }

    public static void ClipTo(this UIElement uiElement, double radiusX,  double radiusY, object bindingSource)
    {
        try
        {
            uiElement.Clip = GetClippedRectangle(radiusX, radiusY, bindingSource);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions ClipTo: " + ex.Message);
        }
    }

    public static ScrollViewer? GetScrollViewer(this UIElement? element)
    {
        if (element == null) return null;
        ScrollViewer? scrollViewer = null;

        try
        {
            int childrenCount = 0;
            element.DispatchIt(() => childrenCount = VisualTreeHelper.GetChildrenCount(element));
            for (int n = 0; n < childrenCount && scrollViewer == null; n++)
            {
                DependencyObject? dependencyObject = null;
                element.DispatchIt(() => dependencyObject = VisualTreeHelper.GetChild(element, n));
                if (dependencyObject != null)
                {
                    if (dependencyObject is ScrollViewer sv)
                    {
                        scrollViewer = sv;
                    }
                    else
                    {
                        scrollViewer = GetScrollViewer(dependencyObject as UIElement);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetScrollViewer: " + ex.Message);
        }

        return scrollViewer;
    }

    /// <summary>
    /// Returns -1 If Fail
    /// </summary>
    public static int GetFirstDisplayedRowIndex(this ScrollViewer? scrollViewer)
    {
        try
        {
            if (scrollViewer != null) return Convert.ToInt32(scrollViewer.VerticalOffset);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetFirstDisplayedRowIndex: " + ex.Message);
        }
        return -1;
    }

    /// <summary>
    /// Returns -1 If Fail
    /// </summary>
    public static int GetLastDisplayedRowIndex(this ScrollViewer? scrollViewer)
    {
        try
        {
            if (scrollViewer != null) return Convert.ToInt32(scrollViewer.VerticalOffset) + Convert.ToInt32(scrollViewer.ViewportHeight) + 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetLastDisplayedRowIndex: " + ex.Message);
        }
        return -1;
    }

    public static void UpdateColumnsWidthToAuto(this DataGrid dataGrid)
    {
        try
        {
            foreach (DataGridColumn column in dataGrid.Columns)
                dataGrid.DispatchIt(() => column.Width = 0);
            dataGrid.DispatchIt(() => dataGrid.UpdateLayout());
            foreach (DataGridColumn column in dataGrid.Columns)
                dataGrid.DispatchIt(() => column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto));
        }
        catch (Exception) { }
    }

    public static void LastColumnFill(this DataGrid dataGrid)
    {
        try
        {
            if (dataGrid.Columns.Count > 0)
            {
                DataGridColumn lastColumn = dataGrid.Columns[^1];
                lastColumn.CanUserResize = true;
                lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions SetLastColumnFillSpaces: " + ex.Message);
        }
    }

    public static (int MinSelectedIndex, int MaxSelectedIndex, int SelectedCount) GetSelectedRowsIndexes(this DataGrid dataGrid)
    {
        int minSelected = dataGrid.SelectedIndex, maxSelected = dataGrid.SelectedIndex, selectedCount = 0;

        try
        {
            selectedCount = dataGrid.SelectedItems.Count;
            for (int n = 0; n < selectedCount; n++)
            {
                object? item = dataGrid.SelectedItems[n];
                if (item == null) continue;
                int itemIndex = dataGrid.Items.IndexOf(item);
                if (itemIndex == -1) continue;
                if (itemIndex < minSelected) minSelected = itemIndex;
                if (itemIndex > maxSelected) maxSelected = itemIndex;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetSelectedRowsIndexes: " + ex.Message);
        }

        return (minSelected, maxSelected, selectedCount);
    }

    /// <summary>
    /// Returns -1 If Fail
    /// </summary>
    public static (int FirstDisplayedRowIndex, int LastDisplayedRowIndex) GetDisplayedRows(this DataGrid dataGrid)
    {
        ScrollViewer? scrollViewer = dataGrid.GetScrollViewer();
        int firstRow = scrollViewer.GetFirstDisplayedRowIndex();
        int lastRow = scrollViewer.GetLastDisplayedRowIndex();
        return (firstRow, lastRow);
    }

    public static void ScrollIntoViewByIndex(this DataGrid dataGrid, int toRowIndex)
    {
        try
        {
            if (toRowIndex >= 0 && toRowIndex < dataGrid.Items.Count)
                dataGrid.ScrollIntoView(dataGrid.Items[toRowIndex]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions ScrollIntoView: " + ex.Message);
        }
    }

    /// <summary>
    /// Animate To Row Index
    /// </summary>
    public static async Task<bool> ScrollIntoViewAsync(this DataGrid dataGrid, int toRowIndex, int doNotAnimateDistanceMoreThanThis = 150)
    {
        try
        {
            if (toRowIndex >= 0 && toRowIndex < dataGrid.Items.Count)
            {
                var (FirstDisplayedRowIndex, LastDisplayedRowIndex) = dataGrid.GetDisplayedRows();
                if (FirstDisplayedRowIndex != -1 && LastDisplayedRowIndex != -1)
                {
                    int fromIndex;
                    int selectedIndex = -1;
                    dataGrid.DispatchIt(() => selectedIndex = dataGrid.SelectedIndex);
                    if (selectedIndex >= FirstDisplayedRowIndex && selectedIndex <= LastDisplayedRowIndex)
                    {
                        fromIndex = selectedIndex;
                    }
                    else
                    {
                        if (toRowIndex >= FirstDisplayedRowIndex && toRowIndex <= LastDisplayedRowIndex)
                        {
                            int average = (FirstDisplayedRowIndex + LastDisplayedRowIndex) / 2;
                            fromIndex = toRowIndex >= average ? FirstDisplayedRowIndex : LastDisplayedRowIndex;
                        }
                        else
                        {
                            fromIndex = toRowIndex < FirstDisplayedRowIndex ? LastDisplayedRowIndex : FirstDisplayedRowIndex;
                        }
                    }

                    if (fromIndex != -1 && fromIndex != toRowIndex)
                    {
                        bool goDown = toRowIndex >= fromIndex;

                        if (goDown)
                        {
                            int distance = toRowIndex - fromIndex;
                            if (distance <= doNotAnimateDistanceMoreThanThis)
                            {
                                int delayMS = distance <= 10 ? 10 : distance <= 20 ? 5 : distance <= 30 ? 2 : 1;
                                for (int n = fromIndex; n <= toRowIndex; n++)
                                {
                                    dataGrid.DispatchIt(() =>
                                    {
                                        dataGrid.SelectedIndex = fromIndex;
                                        dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
                                    });

                                    fromIndex++;
                                    await Task.Delay(delayMS);
                                }
                            }
                            else
                            {
                                dataGrid.DispatchIt(() =>
                                {
                                    dataGrid.SelectedIndex = toRowIndex;
                                    dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
                                });
                            }
                        }
                        else
                        {
                            int distance = fromIndex - toRowIndex;
                            if (distance <= doNotAnimateDistanceMoreThanThis)
                            {
                                int delayMS = distance <= 10 ? 10 : distance <= 20 ? 5 : distance <= 30 ? 2 : 1;
                                for (int n = fromIndex; n >= toRowIndex; n--)
                                {
                                    dataGrid.DispatchIt(() =>
                                    {
                                        dataGrid.SelectedIndex = fromIndex;
                                        dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
                                    });

                                    fromIndex--;
                                    await Task.Delay(delayMS);
                                }
                            }
                            else
                            {
                                dataGrid.DispatchIt(() =>
                                {
                                    dataGrid.SelectedIndex = toRowIndex;
                                    dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
                                });
                            }
                        }
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions ScrollIntoViewAsync: " + ex.Message);
        }
        return false;
    }

    public static DataGridRow? GetRowByIndex(this DataGrid dataGrid, int rowIndex)
    {
        try
        {
            DependencyObject row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (row == null)
            {
                dataGrid.UpdateLayout();
                if (rowIndex >= 0 && rowIndex <= dataGrid.Items.Count - 1)
                {
                    dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                    row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                }
            }
            if (row != null) return row as DataGridRow;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetRowByIndex: " + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// This Is Not Precise
    /// </summary>
    public static DataGridRow? GetRowByCurrentMousePosition(this DataGrid dataGrid)
    {
        try
        {
            Point p = Mouse.GetPosition(dataGrid);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(dataGrid, p);
            if (hitTestResult != null && hitTestResult.VisualHit != null)
                return hitTestResult.VisualHit.GetParentOfType<DataGridRow>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetRowByCurrentMousePosition: " + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// This Is Precise
    /// </summary>
    public static DataGridRow? GetRowByMouseEvent(this DataGrid _, MouseButtonEventArgs e)
    {
        try
        {
            if (e.MouseDevice.DirectlyOver is DependencyObject dependencyObject)
            {
                return dependencyObject.GetParentOfType<DataGridRow>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetRowByMouseEvent: " + ex.Message);
        }
        return null;
    }

    public static DataGridCell? GetCell(this DataGrid dataGrid, DataGridRow row, int columnIndex)
    {
        try
        {
            if (row != null)
            {
                DataGridCellsPresenter? presenter = GetChildOfType<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    dataGrid.UpdateLayout();
                    if (columnIndex >= 0 && columnIndex <= dataGrid.Columns.Count - 1)
                    {
                        dataGrid.ScrollIntoView(row, dataGrid.Columns[columnIndex]);
                        presenter = GetChildOfType<DataGridCellsPresenter>(row);
                    }
                }
                if (presenter != null)
                {
                    DependencyObject cell = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                    if (cell != null) return cell as DataGridCell;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetCell By Row & Column Index: " + ex.Message);
        }
        return null;
    }

    public static DataGridCell? GetCell(this DataGrid dataGrid, int rowIndex, int columnIndex)
    {
        try
        {
            DataGridRow? rowContainer = dataGrid.GetRowByIndex(rowIndex);
            if (rowContainer != null)
            {
                return dataGrid.GetCell(rowContainer, columnIndex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetCell By Row Index & Column Index: " + ex.Message);
        }
        return null;
    }

    public static string GetValue(this DataGridCell cell)
    {
        try
        {
            if (cell.Content is TextBlock textBlock) return textBlock.Text;
            else if (cell.Content is TextBox textBox) return textBox.Text;
            else if (cell.Content is CheckBox checkBox && checkBox.IsChecked.HasValue)
            {
                string? str = checkBox.IsChecked.ToString();
                if (str != null) return str;
            }
            else if (cell.Content is string str) return str;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetValue: " + ex.Message);
        }
        return string.Empty;
    }

    public static bool IsContain(this Visual visual, Point point)
    {
        try
        {
            HitTestResult? result = VisualTreeHelper.HitTest(visual, point);
            return result != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions IsContain: " + ex.Message);
            return false;
        }
    }

    public static bool IsContain2(this Control control, Point point)
    {
        try
        {
            Point buttonPosition = control.PointToScreen(new Point(0, 0));
            return point.X >= buttonPosition.X && point.X <= buttonPosition.X + control.ActualWidth &&
                   point.Y >= buttonPosition.Y && point.Y <= buttonPosition.Y + control.ActualHeight;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions IsContain2: " + ex.Message);
            return false;
        }
    }

    public static void HideHeader(this TabControl tabControl)
    {
        try
        {
            for (int n = 0; n < tabControl.Items.Count; n++)
            {
                if (tabControl.Items[n] is TabItem ti)
                    ti.DispatchIt(() => ti.Visibility = Visibility.Collapsed);
            }
        }
        catch (Exception) { }
    }

    public static void ShowHeader(this TabControl tabControl)
    {
        try
        {
            for (int n = 0; n < tabControl.Items.Count; n++)
            {
                if (tabControl.Items[n] is TabItem ti)
                    ti.DispatchIt(() => ti.Visibility = Visibility.Visible);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Unsubscrib An Event
    /// </summary>
    /// <param name="control">Control</param>
    /// <param name="routedEvent">e.g. TextBox.TextChangedEvent</param>
    /// <param name="filterByEventMethodName">e.g. textBox_TextChanged</param>
    public static void UnsubscribeEvent(this Control control, RoutedEvent routedEvent, string? filterByEventMethodName = null)
    {
        try
        {
            // Get EventHandlersStore Property
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            
            // Get The Instance Of EventHandlersStore
            var eventHandlersStore = eventHandlersStoreProperty?.GetValue(control);
            if (eventHandlersStore == null) return;

            // Use Reflection To Retrieve Handlers For The Event
            MethodInfo? getHandlersMethod = eventHandlersStore.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public); // It's Exposed By BindingFlags.Public
            if (getHandlersMethod == null) return;

            var handlers = (RoutedEventHandlerInfo[]?)getHandlersMethod.Invoke(eventHandlersStore, new object[] { routedEvent });
            if (handlers == null) return;

            foreach (var handler in handlers)
            {
                if (string.IsNullOrEmpty(filterByEventMethodName))
                    control.RemoveHandler(routedEvent, handler.Handler);
                else
                {
                    if (handler.Handler.Method.Name.Contains(filterByEventMethodName))
                        control.RemoveHandler(routedEvent, handler.Handler);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions UnsubscribeEvent: " + ex.Message);
        }
    }

    public static void SetMaxLines(this TextBox textBox, int maxLines, Window owner)
    {
        try
        {
            textBox.DispatchIt(() =>
            {
                textBox.AcceptsReturn = true;
                textBox.MaxLines = maxLines;
            });
            textBox.UnsubscribeEvent(TextBox.TextChangedEvent, nameof(textBox_TextChanged));
            textBox.TextChanged += textBox_TextChanged;
            void textBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                if (sender is not TextBox textBox) return;
                List<string> lines = new();
                int maxLines = -1;
                textBox.DispatchIt(() =>
                {
                    lines = textBox.Text.ReplaceLineEndings().Split(Environment.NewLine).ToList();
                    maxLines = textBox.MaxLines;
                });
                if (lines.Count > maxLines && maxLines != -1)
                {
                    List<string> linesMax = lines.Take(maxLines).ToList();
                    textBox.DispatchIt(() =>
                    {
                        textBox.Text = linesMax.ToString(Environment.NewLine);
                        textBox.CaretIndex = textBox.Text.Length;
                    });
                    string msg = $"You Have Reached Max Line Limits: \"{maxLines}\".";
                    WpfMessageBox.Show(owner, msg, "Line Limits");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions SetMaxLines: " + ex.Message);
        }
    }

    public static Rect GetAbsolutePlacement(this FrameworkElement element, bool relativeToScreen = false)
    {
        Rect rect = new();

        try
        {
            Point elementPTS = element.PointToScreen(new Point(0, 0));
            if (relativeToScreen)
            {
                return new Rect(elementPTS.X, elementPTS.Y, element.ActualWidth, element.ActualHeight);
            }
            Window? parentWindow = element.GetParentWindow();
            if (parentWindow != null)
            {
                Point containerPTS = parentWindow.PointToScreen(new Point(0, 0));
                Point absolutePos = new(elementPTS.X - containerPTS.X, elementPTS.Y - containerPTS.Y);
                rect = new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions GetAbsolutePlacement: " + ex.Message);
        }

        return rect;
    }

    public static bool HitTest<T>(this UIElement uiElement, Point point)
    {
        try
        {
            DependencyObject? hitTestResult = null;

            HitTestResultBehavior resultCallback(HitTestResult result)
            {
                if (result.VisualHit is UIElement visualHitElement && visualHitElement.Visibility == Visibility.Visible)
                {
                    hitTestResult = result.VisualHit;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }

            HitTestFilterBehavior filterCallBack(DependencyObject potentialHitTestTarget)
            {
                if (potentialHitTestTarget is T)
                {
                    hitTestResult = potentialHitTestTarget;
                    return HitTestFilterBehavior.Stop;
                }
                return HitTestFilterBehavior.Continue;
            }

            Window? parentWindow = uiElement.GetParentWindow();
            if (parentWindow == null) return false;
            PointHitTestParameters parameter = new(point);
            VisualTreeHelper.HitTest(parentWindow, filterCallBack, resultCallback, parameter);
            if (hitTestResult == null) return false;
            if (hitTestResult.GetHashCode() == uiElement.GetHashCode()) return true;
            return Controllers.IsElementChildOfParent(uiElement, hitTestResult);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions HitTest: " + ex.Message);
            return false;
        }
    }

    public static bool IsVisibleToUser<T>(this FrameworkElement? element)
    {
        try
        {
            if (element == null) return false;
            Rect rect = element.GetAbsolutePlacement();
            Point pTopLeft = new(rect.TopLeft.X + 1, rect.TopLeft.Y + 1);
            Point pBottomRight = new(rect.BottomRight.X - 1, rect.BottomRight.Y - 1);
            bool isTopLeftVisible = HitTest<T>(element, pTopLeft);
            bool isBottomRightVisible = HitTest<T>(element, pBottomRight);
            return isTopLeftVisible && isBottomRightVisible;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppExtensions HitTest: " + ex.Message);
            return false;
        }
    }

}