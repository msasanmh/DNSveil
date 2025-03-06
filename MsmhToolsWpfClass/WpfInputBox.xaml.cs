using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfInputBox.xaml
/// </summary>
public partial class WpfInputBox : Window
{
    public new bool? DialogResult { get; private set; } = false; // For Animation Sake!
    private string Description { get; set; }
    private string Input { get; set; } = string.Empty;
    private bool Multiline { get; set; }

    private WpfInputBox(ref string input, string description, string title, bool multiline, Window owner)
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfInputBox), new FrameworkPropertyMetadata(typeof(WpfInputBox)));
        Opacity = 0;
        PART_InputTextBox.Text = input;
        Title = title;
        Description = description;
        Multiline = multiline;
        Owner = owner;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;

        try
        {
            DoubleAnimation anim = new(1, new Duration(TimeSpan.FromMilliseconds(200)));
            BeginAnimation(OpacityProperty, anim);
        }
        catch (Exception) { }
    }

    public static bool Show(Window owner, ref string input, string description)
    {
        WpfInputBox inputBox = new(ref input, description, "Input Box", false, owner);
        inputBox.ShowDialog();
        bool dialogResult = inputBox.DialogResult ??= false;
        if (dialogResult) input = inputBox.Input;
        return dialogResult;
    }

    public static bool Show(Window owner, ref string input, string description, string title)
    {
        WpfInputBox inputBox = new(ref input, description, title, false, owner);
        inputBox.ShowDialog();
        bool dialogResult = inputBox.DialogResult ??= false;
        if (dialogResult) input = inputBox.Input;
        return dialogResult;
    }

    public static bool Show(Window owner, ref string input, string description, bool multiline)
    {
        WpfInputBox inputBox = new(ref input, description, "Input Box", multiline, owner);
        inputBox.ShowDialog();
        bool dialogResult = inputBox.DialogResult ??= false;
        if (dialogResult) input = inputBox.Input;
        return dialogResult;
    }

    public static bool Show(Window owner, ref string input, string description, string title, bool multiline)
    {
        WpfInputBox inputBox = new(ref input, description, title, multiline, owner);
        inputBox.ShowDialog();
        bool dialogResult = inputBox.DialogResult ??= false;
        if (dialogResult) input = inputBox.Input;
        return dialogResult;
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
            PART_InputTextBox.AcceptsReturn = Multiline;
        }
        catch (Exception) { }
    }

    private void MainWindow_ContentRendered(object sender, EventArgs e)
    {
        PART_InputTextBox.Focus();
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
        Input = PART_InputTextBox.Text.Trim();
        DialogResult = true;
        Close();
    }

    private void PART_CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
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