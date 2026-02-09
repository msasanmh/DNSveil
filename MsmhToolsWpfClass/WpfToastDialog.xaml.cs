using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfToastDialog.xaml
/// </summary>
public partial class WpfToastDialog : Window
{
    public enum Location
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    private string Message { get; set; }
    private MessageBoxImage BoxImage { get; set; } = MessageBoxImage.None;
    private Icon BoxIcon { get; set; } = SystemIcons.Application;
    private Location DialogLocation { get; set; } = Location.BottomCenter;
    private Color MessageColor { get; set; } = Colors.MediumSeaGreen;

    private WpfToastDialog(string message, MessageBoxImage messageBoxImage, Location dialogLocation, Window owner)
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfToastDialog), new FrameworkPropertyMetadata(typeof(WpfToastDialog)));
        Opacity = 0;
        Message = message;
        BoxImage = messageBoxImage;
        DialogLocation = dialogLocation;
        Owner = owner;
        WindowStyle = WindowStyle.None;
        UseLayoutRounding = true;
        ShowInTaskbar = false;
        Owner.LocationChanged -= Owner_LocationChanged;
        Owner.LocationChanged += Owner_LocationChanged;
        Owner.SizeChanged -= Owner_SizeChanged;
        Owner.SizeChanged += Owner_SizeChanged;

        try
        {
            DoubleAnimation anim = new(1, (Duration)TimeSpan.FromMilliseconds(300));
            BeginAnimation(OpacityProperty, anim);
        }
        catch (Exception) { }
    }

    public static async void Show(Window owner, string message, MessageBoxImage messageBoxImage = MessageBoxImage.None, int timeoutSec = 3, Location dialogLocation = Location.BottomCenter)
    {
        try
        {
            WpfToastDialog box = new(message, messageBoxImage, dialogLocation, owner);
            box.Show();
            box.Owner.Focus();
            await Task.Delay(timeoutSec * 1000);
            box.Close();
        }
        catch (Exception) { }
    }

    private Point GetLocation(Location location)
    {
        double x = 0, y = 0;
        try
        {
            UpdateLayout();
            double ownerLeft = Owner.Left, ownerTop = Owner.Top;
            double borderlessMargin = 7, customPadding = 50;
            WindowState ownerWindowState = Owner.WindowState;
            WindowStyle ownerWindowStyle = Owner.WindowStyle;
            ResizeMode ownerResizeMode = Owner.ResizeMode;

            if (ownerWindowState == WindowState.Maximized)
            {
                ownerLeft = 0;
                ownerTop = 0;
            }

            if (ownerWindowStyle == WindowStyle.None && ownerResizeMode == ResizeMode.NoResize)
            {
                borderlessMargin = 1;
            }

            if (Owner is WpfWindow && ownerWindowState != WindowState.Maximized)
            {
                borderlessMargin = 0;
            }

            if (location == Location.TopLeft)
            {
                x = ownerLeft + borderlessMargin + customPadding;
                y = ownerTop + customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin;
                }
            }
            else if (location == Location.TopCenter)
            {
                x = ownerLeft + (Owner.ActualWidth / 2) - (ActualWidth / 2);
                y = ownerTop + customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                }
            }
            else if (location == Location.TopRight)
            {
                x = ownerLeft - borderlessMargin + Owner.ActualWidth - ActualWidth - customPadding;
                y = ownerTop + customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                }
            }
            else if (location == Location.MiddleLeft)
            {
                x = ownerLeft + borderlessMargin + customPadding;
                y = ownerTop - (borderlessMargin / 2) + (Owner.ActualHeight / 2) - (ActualHeight / 2);
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin;
                    y -= (borderlessMargin / 2) + 2;
                }
            }
            else if (location == Location.MiddleCenter)
            {
                x = ownerLeft + (Owner.ActualWidth / 2) - (ActualWidth / 2);
                y = ownerTop - (borderlessMargin / 2) + (Owner.ActualHeight / 2) - (ActualHeight / 2);
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                    y -= (borderlessMargin / 2) + 2;
                }
            }
            else if (location == Location.MiddleRight)
            {
                x = ownerLeft - borderlessMargin + Owner.ActualWidth - ActualWidth - customPadding;
                y = ownerTop - (borderlessMargin / 2) + (Owner.ActualHeight / 2) - (ActualHeight / 2);
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                    y -= (borderlessMargin / 2) + 2;
                }
            }
            else if (location == Location.BottomLeft)
            {
                x = ownerLeft + borderlessMargin + customPadding;
                y = ownerTop - borderlessMargin + Owner.ActualHeight - ActualHeight - customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin;
                    y -= borderlessMargin + 2;
                }
            }
            else if (location == Location.BottomCenter)
            {
                x = ownerLeft + (Owner.ActualWidth / 2) - (ActualWidth / 2);
                y = ownerTop - borderlessMargin + Owner.ActualHeight - ActualHeight - customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                    y -= borderlessMargin + 2;
                }
            }
            else if (location == Location.BottomRight)
            {
                x = ownerLeft - borderlessMargin + Owner.ActualWidth - ActualWidth - customPadding;
                y = ownerTop - borderlessMargin + Owner.ActualHeight - ActualHeight - customPadding;
                if (ownerWindowState == WindowState.Maximized)
                {
                    x -= borderlessMargin + 2;
                    y -= borderlessMargin + 2;
                }
            }
        }
        catch (Exception) { }
        return new Point(x, y);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            double width = SystemParameters.PrimaryScreenWidth * 0.5;
            double height = SystemParameters.PrimaryScreenHeight * 0.5;
            Width = width;
            Height = height;
            MaxWidth = width;
            MaxHeight = height;

            if (BoxImage == MessageBoxImage.None)
            {
                BoxIcon = this.GetApplicationIcon();
            }
            else if (BoxImage == MessageBoxImage.Asterisk ||
                    BoxImage == MessageBoxImage.Information)
            {
                BoxIcon = SystemIcons.Asterisk;
            }
            else if (BoxImage == MessageBoxImage.Error ||
                    BoxImage == MessageBoxImage.Hand ||
                    BoxImage == MessageBoxImage.Stop)
            {
                BoxIcon = SystemIcons.Error;
                MessageColor = Colors.IndianRed;
            }
            else if (BoxImage == MessageBoxImage.Exclamation ||
                    BoxImage == MessageBoxImage.Warning)
            {
                BoxIcon = SystemIcons.Exclamation;
                MessageColor = Colors.Orange;
            }
            else if (BoxImage == MessageBoxImage.Question)
            {
                BoxIcon = SystemIcons.Question;
            }

            PART_Image.Source = BoxIcon.ToImageSource();

            PART_MessageTextBlock.Foreground = new SolidColorBrush(MessageColor);
            PART_MessageTextBlock.Text = Message;

            Point location = GetLocation(DialogLocation);
            Left = location.X;
            Top = location.Y;
        }
        catch (Exception) { }
    }

    private void Owner_LocationChanged(object? sender, EventArgs e)
    {
        try
        {
            Point location = GetLocation(DialogLocation);
            Left = location.X;
            Top = location.Y;
        }
        catch (Exception) { }
    }

    private void Owner_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            Point location = GetLocation(DialogLocation);
            Left = location.X;
            Top = location.Y;
        }
        catch (Exception) { }
    }

    private bool IsClosing = false;
    private bool IsDisposed = false;

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            if (IsClosing && !IsDisposed)
            {
                e.Cancel = true;
                return;
            }

            if (!IsClosing)
            {
                e.Cancel = true;
                IsClosing = true;

                // Start Custom Dispose
                BoxIcon.Dispose();
                // End Custom Dispose

                DoubleAnimation anim = new(0, (Duration)TimeSpan.FromMilliseconds(300));
                anim.Completed += async (s, _) =>
                {
                    GC.Collect();
                    await Task.Delay(10);
                    IsDisposed = true;
                    try
                    {
                        Close();
                    }
                    catch (Exception)
                    {
                        await Task.Delay(10);
                        Close();
                    }
                };
                BeginAnimation(OpacityProperty, anim);
            }
        }
        catch (Exception) { }
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        try
        {
            Owner.Focus();
        }
        catch (Exception) { }
    }
}