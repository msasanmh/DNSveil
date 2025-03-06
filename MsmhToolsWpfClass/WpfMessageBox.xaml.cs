using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfMessageBox.xaml
/// </summary>
public partial class WpfMessageBox : Window
{
    private string Description { get; set; }
    private MessageBoxButton BoxButton { get; set; } = MessageBoxButton.OK;
    private MessageBoxImage BoxImage { get; set; } = MessageBoxImage.None;
    private Icon BoxIcon { get; set; } = SystemIcons.Application;
    private MessageBoxResult BoxResult { get; set; } = MessageBoxResult.None;
    private MessageBoxOptions BoxOptions { get; set; } = MessageBoxOptions.None;

    private WpfMessageBox(string description, string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, Window? owner)
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfMessageBox), new FrameworkPropertyMetadata(typeof(WpfMessageBox)));
        Opacity = 0;
        Title = title;
        Description = description;
        BoxButton = messageBoxButton;
        BoxImage = messageBoxImage;
        owner ??= Application.Current.MainWindow;
        Owner = owner;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        UseLayoutRounding = true;

        try
        {
            DoubleAnimation anim = new(1, new Duration(TimeSpan.FromMilliseconds(200)));
            BeginAnimation(OpacityProperty, anim);
        }
        catch (Exception) { }
    }

    public static MessageBoxResult Show(Window? owner, string description)
    {
        WpfMessageBox box = new(description, "Message", MessageBoxButton.OK, MessageBoxImage.None, owner);
        box.ShowDialog();
        return box.BoxResult;
    }

    public static MessageBoxResult Show(Window? owner, string description, string title)
    {
        WpfMessageBox box = new(description, title, MessageBoxButton.OK, MessageBoxImage.None, owner);
        box.ShowDialog();
        return box.BoxResult;
    }

    public static MessageBoxResult Show(Window? owner, string description, string title, MessageBoxButton messageBoxButton)
    {
        WpfMessageBox box = new(description, title, messageBoxButton, MessageBoxImage.None, owner);
        box.ShowDialog();
        return box.BoxResult;
    }

    public static MessageBoxResult Show(Window? owner, string description, string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
    {
        WpfMessageBox box = new(description, title, messageBoxButton, messageBoxImage, owner);
        box.ShowDialog();
        return box.BoxResult;
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

            PART_OkButton.IsDefault = true;
            PART_CancelButton.IsCancel = true;

            PART_TitleTextBlock.FontSize = PART_DescriptionTextBlock.FontSize + 4;
            PART_TitleTextBlock.Text = Title;
            PART_DescriptionTextBlock.Text = Description;

            if (BoxButton == MessageBoxButton.OK)
            {
                PART_OkButton.Visibility = Visibility.Visible;
                PART_OkButton.Focus();
                PART_YesButton.Visibility = Visibility.Collapsed;
                PART_NoButton.Visibility = Visibility.Collapsed;
                PART_CancelButton.Visibility = Visibility.Collapsed;
            }
            else if (BoxButton == MessageBoxButton.OKCancel)
            {
                PART_OkButton.Visibility = Visibility.Visible;
                PART_OkButton.Focus();
                PART_YesButton.Visibility = Visibility.Collapsed;
                PART_NoButton.Visibility = Visibility.Collapsed;
                PART_CancelButton.Visibility = Visibility.Visible;
            }
            else if (BoxButton == MessageBoxButton.YesNo)
            {
                PART_OkButton.Visibility = Visibility.Collapsed;
                PART_YesButton.Visibility = Visibility.Visible;
                PART_YesButton.Focus();
                PART_NoButton.Visibility = Visibility.Visible;
                PART_CancelButton.Visibility = Visibility.Collapsed;
            }
            else if (BoxButton == MessageBoxButton.YesNoCancel)
            {
                PART_OkButton.Visibility = Visibility.Collapsed;
                PART_YesButton.Visibility = Visibility.Visible;
                PART_YesButton.Focus();
                PART_NoButton.Visibility = Visibility.Visible;
                PART_CancelButton.Visibility = Visibility.Visible;
            }

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
            }
            else if (BoxImage == MessageBoxImage.Exclamation ||
                    BoxImage == MessageBoxImage.Warning)
            {
                BoxIcon = SystemIcons.Exclamation;
            }
            else if (BoxImage == MessageBoxImage.Question)
            {
                BoxIcon = SystemIcons.Question;
            }

            PART_Image.Source = BoxIcon.ToImageSource();
        }
        catch (Exception) { }
    }

    public override void OnApplyTemplate()
    {
        this.ClipTo(5, 5, this);
        base.OnApplyTemplate();
    }

    private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                if (e.ButtonState == MouseButtonState.Pressed) DragMove();
            }
            catch (Exception) { }
        }
    }

    private void PART_OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BoxResult = MessageBoxResult.OK;
        Close();
    }

    private void PART_YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BoxResult = MessageBoxResult.Yes;
        Close();
    }

    private void PART_NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BoxResult = MessageBoxResult.No;
        Close();
    }

    private void PART_CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        BoxResult = MessageBoxResult.Cancel;
        Close();
    }

    private bool IsClosing = false;
    private bool IsDisposed = false;

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
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

                DoubleAnimation anim = new(0, new Duration(TimeSpan.FromMilliseconds(200)));
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

}