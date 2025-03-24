using MsmhToolsClass;
using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfWindow.xaml
/// </summary>
public partial class WpfWindow : Window
{
    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfWindow),
            new PropertyMetadata(new CornerRadius(0)));

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ReadOnly(true)]
    public double GetMaxCornerRadius
    {
        get
        {
            double r = 0;
            try
            {
                double r1 = Math.Max(CornerRadius.TopLeft, CornerRadius.TopRight);
                double r2 = Math.Max(CornerRadius.BottomRight, CornerRadius.BottomLeft);
                r = Math.Max(r1, r2);
            }
            catch (Exception) { }
            return r;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ReadOnly(true)]
    public double GetMinCornerRadius
    {
        get
        {
            double r = 0;
            try
            {
                double r1 = Math.Min(CornerRadius.TopLeft, CornerRadius.TopRight);
                double r2 = Math.Min(CornerRadius.BottomRight, CornerRadius.BottomLeft);
                r = Math.Min(r1, r2);
            }
            catch (Exception) { }
            return r;
        }
    }

    private readonly MenuItem SystemMenu_MenuItem = new();
    private readonly ContextMenu SystemMenu_ContextMenu = new();

    private bool IsTitleGridLeftMouseDownPressed = false;
    private Border? PART_RootBorder;
    private Grid? PART_RootGrid;
    private Grid? PART_TitleGrid;
    private Image? PART_TitleImage;
    private WpfButton? PART_MinimizeButton;
    private WpfButton? PART_MaximizeRestoreButton;
    private WpfButton? PART_CloseButton;

    /// <summary>
    /// Customize On Window.Loaded Event.
    /// </summary>
    public WpfButton? PART_Button1 { get; set; }
    /// <summary>
    /// Customize On Window.Loaded Event.
    /// </summary>
    public WpfButton? PART_Button2 { get; set; }
    /// <summary>
    /// Customize On Window.Loaded Event.
    /// </summary>
    public WpfButton? PART_Button3 { get; set; }
    public Separator? PART_Separator { get; set; }

    public WpfWindow()
    {
        try
        {
            DefaultStyleKey = typeof(WpfWindow);
        }
        catch (Exception) { }
        Background = Brushes.DarkGray;
        Foreground = Brushes.White;
        DataContext = this;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = GetMinCornerRadius > 0;
        UseLayoutRounding = true; // Ensuring That Everything Aligns Nicely On The High-DPI Screens
        Icon = this.GetApplicationIcon().ToImageSource();
        SystemMenu_MenuItem.Header = "Show System Menu";
        SystemMenu_ContextMenu.Items.Add(SystemMenu_MenuItem);
        Loaded -= WpfWindow_Loaded;
        Loaded += WpfWindow_Loaded;
        StateChanged -= WpfWindow_StateChanged;
        StateChanged += WpfWindow_StateChanged;
    }

    private void WpfWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Background = AppTheme.GetBrush(AppTheme.BackgroundBrush);
        Foreground = AppTheme.GetBrush(AppTheme.ForegroundBrush);

        EnableDefaultWindowAnimations(new WindowInteropHelper(this).Handle);
        EnableSnapLayout();
    }

    private void WpfWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized) IsTitleGridLeftMouseDownPressed = false;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        Background = AppTheme.GetBrush(AppTheme.BackgroundBrush);
        Foreground = AppTheme.GetBrush(AppTheme.ForegroundBrush);

        base.OnContentRendered(e);
        if (SizeToContent == SizeToContent.WidthAndHeight)
            InvalidateMeasure();
    }

    public override void OnApplyTemplate()
    {
        PART_RootBorder = GetTemplateChild(nameof(PART_RootBorder)) as Border;
        PART_RootBorder?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_RootBorder);

        PART_RootGrid = GetTemplateChild(nameof(PART_RootGrid)) as Grid;
        PART_RootGrid?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_RootGrid);

        PART_TitleGrid = GetTemplateChild(nameof(PART_TitleGrid)) as Grid;
        if (PART_TitleGrid != null)
        {
            PART_TitleGrid.MouseDown += PART_TitleGrid_MouseDown;
            PART_TitleGrid.MouseUp += PART_TitleGrid_MouseUp;
            PART_TitleGrid.MouseMove += PART_TitleGrid_MouseMove;
        }

        PART_TitleImage = GetTemplateChild(nameof(PART_TitleImage)) as Image;
        if (PART_TitleImage != null)
        {
            PART_TitleImage.PreviewMouseDown += PART_TitleImage_PreviewMouseDown;
            PART_TitleImage.MouseDown += PART_TitleImage_MouseDown;
        }

        PART_MinimizeButton = GetTemplateChild(nameof(PART_MinimizeButton)) as WpfButton;
        if (PART_MinimizeButton != null) PART_MinimizeButton.Click += PART_MinimizeButton_Click;

        PART_MaximizeRestoreButton = GetTemplateChild(nameof(PART_MaximizeRestoreButton)) as WpfButton;
        if (PART_MaximizeRestoreButton != null) PART_MaximizeRestoreButton.Click += PART_MaximizeRestoreButton_Click;

        PART_CloseButton = GetTemplateChild(nameof(PART_CloseButton)) as WpfButton;
        if (PART_CloseButton != null) PART_CloseButton.Click += PART_CloseButton_Click;

        // Custom Buttons
        PART_Button1 = GetTemplateChild(nameof(PART_Button1)) as WpfButton;
        PART_Button2 = GetTemplateChild(nameof(PART_Button2)) as WpfButton;
        PART_Button3 = GetTemplateChild(nameof(PART_Button3)) as WpfButton;
        PART_Separator = GetTemplateChild(nameof(PART_Separator)) as Separator;
        if (PART_Button1 != null && PART_Button2 != null && PART_Button3 != null)
        {
            PART_Button1.IsVisibleChanged += PART_Button_IsVisibleChanged;
            PART_Button2.IsVisibleChanged += PART_Button_IsVisibleChanged;
            PART_Button3.IsVisibleChanged += PART_Button_IsVisibleChanged;

            PART_Button1.Visibility = Visibility.Collapsed;
            PART_Button2.Visibility = Visibility.Collapsed;
            PART_Button3.Visibility = Visibility.Collapsed;
        }

        base.OnApplyTemplate();
    }

    private void PART_Button_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        try
        {
            if (PART_Button1 == null || PART_Button2 == null || PART_Button3 == null || PART_Separator == null) return;
            if (PART_Button1.Visibility == Visibility.Visible || PART_Button2.Visibility == Visibility.Visible || PART_Button3.Visibility == Visibility.Visible)
                PART_Separator.Visibility = Visibility.Visible;
            else
                PART_Separator.Visibility = Visibility.Collapsed;
        }
        catch (Exception) { }
    }

    private void PART_MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Minimize();
    }

    private void PART_MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        MaximizeRestore();
    }

    private void PART_CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseMe();
    }

    private void PART_TitleGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    MaximizeRestore();
                    return;
                }
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    IsTitleGridLeftMouseDownPressed = true;
                }
            }
            catch (Exception) { }
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            OpenSystemContextMenu(e);
        }
    }

    private void PART_TitleGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        IsTitleGridLeftMouseDownPressed = false;
    }

    private void PART_TitleGrid_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            if (IsTitleGridLeftMouseDownPressed)
            {
                if (WindowState == WindowState.Maximized) MaximizeRestore();
                if (e.LeftButton == MouseButtonState.Pressed) DragMove();
            }
        }
        catch (Exception) { }
    }

    private void PART_TitleImageLabel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
        {
            Debug.WriteLine("SINGLE CLICK");
            OpenSystemContextMenu(e);
        }
    }

    private void PART_TitleImageLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Debug.WriteLine("DOUBLE CLICK");
            CloseMe();
            e.Handled = true;
        }
    }

    private void PART_TitleImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
        {
            CloseMe();
            e.Handled = true;
        }
    }

    private async void PART_TitleImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
        {
            await Task.Delay(300);
            OpenSystemContextMenu(e);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action PAction;
        public event EventHandler? CanExecuteChanged = (s, e) => { };
        public RelayCommand(Action action) { PAction = action; }
        public bool CanExecute(object? parameter) { return true; }
        public void Execute(object? parameter) { PAction(); }
    }

    private void OpenSystemContextMenu(MouseButtonEventArgs e)
    {
        try
        {
            bool showSystemMenu = false;
            if (!showSystemMenu) return;
            Point mousePosition = e.GetPosition(this);
            if (mousePosition.X == 0 || mousePosition.Y == 0) return;
            Point pointToScreen = PointToScreen(mousePosition);
            double offsetX = 5, offsetY = 5;
            Point point = new(pointToScreen.X + offsetX, pointToScreen.Y + offsetY);

            SystemMenu_MenuItem.Command = new RelayCommand(() => SystemCommands.ShowSystemMenu(this, point));
            SystemMenu_ContextMenu.IsOpen = true;
            //SystemCommands.ShowSystemMenu(this, point); // Bug: Opens Other Menus Sometimes
        }
        catch (Exception) { }
    }

    private void Minimize()
    {
        try
        {
            bool canMinimize = ResizeMode != ResizeMode.NoResize;
            if (canMinimize) SystemCommands.MinimizeWindow(this);
        }
        catch (Exception) { }
    }

    private void MaximizeRestore()
    {
        try
        {
            bool canResize = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
            if (canResize)
            {
                if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
                else SystemCommands.MaximizeWindow(this);
            }
        }
        catch (Exception) { }
    }

    private async void CloseMe()
    {
        try
        {
            Close();
        }
        catch (Exception)
        {
            try
            {
                await Task.Delay(10);
                Close();
            }
            catch (Exception) { }
        }
    }

    public static void EnableDefaultWindowAnimations(IntPtr hWnd, int nIndex = -16)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr dwNewLong = new((long)(NativeMethods.WS.WS_CAPTION | NativeMethods.WS.WS_CLIPCHILDREN | NativeMethods.WS.WS_MINIMIZEBOX | NativeMethods.WS.WS_MAXIMIZEBOX | NativeMethods.WS.WS_SYSMENU | NativeMethods.WS.WS_SIZEBOX));
                HandleRef handle = new(null, hWnd);
                if (IntPtr.Size == 8)
                    _ = NativeMethods.SetWindowLongPtr64(handle, nIndex, dwNewLong);
                else
                    _ = NativeMethods.SetWindowLong32(handle, nIndex, dwNewLong.ToInt32());
            }
        }
        catch (Exception) { }
    }

    //== Snap Layout Support ==//
    private float ScaleFactor = 1;
    private HwndSource? HwndSource;

    /// <summary>
    /// This Needs To Be Called In Window.Loaded Event.
    /// </summary>
    private void EnableSnapLayout()
    {
        try
        {
            // This Feature Is Only Available On Windows 11+
            if (!OsTool.IsWindows11OrGreater()) return;
            if (ResizeMode == ResizeMode.CanMinimize || ResizeMode == ResizeMode.NoResize) return;

            PresentationSource source = PresentationSource.FromVisual(this);
            double dpi = 96.0 * source.CompositionTarget.TransformToDevice.M11;

            ScaleFactor = (float)(dpi / 96.0);
            HwndSource = PresentationSource.FromVisual(this) as HwndSource;
            HwndSource?.AddHook(HwndSourceHook);
        }
        catch (Exception) { }
    }

    private bool IsCursorOnButton(IntPtr lparam, WpfButton button)
    {
        try
        {
            // Extract Mouse Coordinates From Lparam
            int mouseX = (short)(lparam.ToInt32() & 0xFFFF);
            int mouseY = (short)((lparam.ToInt32() >> 16) & 0xFFFF);

            // Get Button's Actual Dimensions And Position
            Point buttonPosition = button.PointToScreen(new Point(0, 0));

            // Check If Match
            bool result = mouseX >= buttonPosition.X && mouseX <= buttonPosition.X + button.ActualWidth * ScaleFactor &&
                          mouseY >= buttonPosition.Y && mouseY <= buttonPosition.Y + button.ActualHeight * ScaleFactor;
            //Debug.WriteLine("On Button: " + result);
            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void GetButtonState(WpfButton button, out bool isMouseOver, out bool isPressed)
    {
        isMouseOver = button.IsMouseOver;
        isPressed = button.IsPressed;
    }

    private static void SetButtonState(WpfButton button, bool? setIsMouseOver = null, bool? setIsPressed = null)
    {
        try
        {
            DependencyPropertyKey? UIElementIsMouseOverPropertyKey = (DependencyPropertyKey?)typeof(UIElement).GetField("IsMouseOverPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null);
            DependencyPropertyKey? ButtonIsPressedPropertyKey = (DependencyPropertyKey?)typeof(ButtonBase).GetField("IsPressedPropertyKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null);

            if (setIsMouseOver.HasValue && UIElementIsMouseOverPropertyKey != null) button.SetValue(UIElementIsMouseOverPropertyKey, setIsMouseOver);
            if (setIsPressed.HasValue && ButtonIsPressedPropertyKey != null) button.SetValue(ButtonIsPressedPropertyKey, setIsPressed.Value);

            // Get Actual States
            GetButtonState(button, out bool isMouseOver, out bool isPressed);

            string state = "Normal";
            if (isMouseOver) state = "MouseOver";
            else if (isPressed) state = "Pressed";

            // Apply Visual State (For Styling To Work)
            VisualStateManager.GoToState(button, state, true);
        }
        catch (Exception) { }
    }

    private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        try
        {
            // Only For WindowsStyle = None
            if (WindowStyle != WindowStyle.None) return IntPtr.Zero;
            if (PART_MaximizeRestoreButton == null) return IntPtr.Zero;

            const int WM_NCHITTEST = 0x0084; // Interop Values
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int WM_NCLBUTTONUP = 0x00A2;
            const int HTMAXBUTTON = 9;

            //https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-snap-layout-menu
            //https://github.com/dotnet/wpf/issues/4825

            switch (msg)
            {
                case WM_NCHITTEST:
                    if (IsCursorOnButton(lparam, PART_MaximizeRestoreButton))
                    {
                        SetButtonState(PART_MaximizeRestoreButton, setIsMouseOver: true);
                        handled = true;
                        return new IntPtr(HTMAXBUTTON);
                    }
                    else
                    {
                        SetButtonState(PART_MaximizeRestoreButton, setIsMouseOver: false, setIsPressed: false);
                    }
                    break;
                case WM_NCLBUTTONDOWN:
                    if (IsCursorOnButton(lparam, PART_MaximizeRestoreButton))
                    {
                        SetButtonState(PART_MaximizeRestoreButton, setIsPressed: true);
                        handled = true;
                    }
                    break;
                case WM_NCLBUTTONUP:
                    if (IsCursorOnButton(lparam, PART_MaximizeRestoreButton))
                    {
                        GetButtonState(PART_MaximizeRestoreButton, out _, out bool isPressed);
                        SetButtonState(PART_MaximizeRestoreButton, setIsMouseOver: false, setIsPressed: false);
                        handled = true;
                        if (isPressed)
                        {
                            // Fire Button Click
                            PART_MaximizeRestoreButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        }
                    }
                    break;
            }
        }
        catch (Exception) { }

        return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            HwndSource?.RemoveHook(HwndSourceHook);
            HwndSource = null;
        }
        catch (Exception) { }
        base.OnClosed(e);
    }

}