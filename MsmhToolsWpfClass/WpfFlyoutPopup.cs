using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

public class WpfFlyoutPopup : GroupBox
{
    public class FlyoutChangedEventArgs : EventArgs
    {
        public bool IsFlyoutOpen { get; private set; }

        public FlyoutChangedEventArgs(bool isFlyoutOpen)
        {
            IsFlyoutOpen = isFlyoutOpen;
        }
    }
    public event EventHandler<FlyoutChangedEventArgs>? FlyoutChanged;

    [Category("Brush")]
    public Brush HeaderBrush
    {
        get { return (Brush)GetValue(HeaderBrushProperty); }
        set { SetValue(HeaderBrushProperty, value); }
    }
    public static readonly DependencyProperty HeaderBrushProperty =
        DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(WpfFlyoutPopup),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrushTransparent)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfFlyoutPopup),
            new PropertyMetadata(new CornerRadius(5)));

    [Category("Common")]
    public object Header2
    {
        get { return GetValue(Header2Property); }
        set { SetValue(Header2Property, value); }
    }
    public static readonly DependencyProperty Header2Property =
        DependencyProperty.Register(nameof(Header2), typeof(object), typeof(WpfFlyoutPopup), new PropertyMetadata(null));

    [Category("Common")]
    public bool StaysOpen
    {
        get { return (bool)GetValue(StaysOpenProperty); }
        set { SetValue(StaysOpenProperty, value); }
    }
    public static readonly DependencyProperty StaysOpenProperty =
        DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(WpfFlyoutPopup), new PropertyMetadata(false));

    [Category("Common")]
    public PlacementMode Placement
    {
        get { return (PlacementMode)GetValue(PlacementProperty); }
        set { SetValue(PlacementProperty, value); }
    }
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(WpfFlyoutPopup),
            new PropertyMetadata(PlacementMode.Bottom, (s, e) =>
            {
                if (s is WpfFlyoutPopup wpfFlyoutOverlay)
                {
                    wpfFlyoutOverlay.SetPlacement();
                }
            }));

    public int AnimDurationMS { get; set; } = 200;

    public bool IsOpen { get; private set; }

    private Window? ParentWindow;
    private StackPanel? PART_HeaderStackPanel;
    private Ellipse? PART_CircleEllipse;
    private Path? PART_ArrowPath;
    private Popup? PART_Popup;
    private WpfButton? PART_CloseButton;

    private readonly Geometry PathOpen = Geometry.Parse("M 5 1 L 5 9 L 7 7 L 7 8 L 4 12 L 1 8 L 1 7 L 3 9 L 3 1 L 5 1 Z");

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ParentWindow = this.GetParentWindow();
        if (ParentWindow != null)
        {
            ParentWindow.Activated -= ParentWindow_Activated;
            ParentWindow.Activated += ParentWindow_Activated;
            ParentWindow.Deactivated -= ParentWindow_Deactivated;
            ParentWindow.Deactivated += ParentWindow_Deactivated;
            ParentWindow.LocationChanged -= ParentWindow_LocationChanged;
            ParentWindow.LocationChanged += ParentWindow_LocationChanged;
            ParentWindow.SizeChanged -= ParentWindow_SizeChanged;
            ParentWindow.SizeChanged += ParentWindow_SizeChanged;
        }

        PART_HeaderStackPanel = GetTemplateChild(nameof(PART_HeaderStackPanel)) as StackPanel;
        if (PART_HeaderStackPanel != null)
        {
            PART_HeaderStackPanel.PreviewMouseLeftButtonDown -= PART_HeaderStackPanel_PreviewMouseLeftButtonDown;
            PART_HeaderStackPanel.PreviewMouseLeftButtonDown += PART_HeaderStackPanel_PreviewMouseLeftButtonDown;
            PART_HeaderStackPanel.IsEnabledChanged -= PART_HeaderStackPanel_IsEnabledChanged;
            PART_HeaderStackPanel.IsEnabledChanged += PART_HeaderStackPanel_IsEnabledChanged;
        }

        PART_CircleEllipse = GetTemplateChild(nameof(PART_CircleEllipse)) as Ellipse;

        PART_ArrowPath = GetTemplateChild(nameof(PART_ArrowPath)) as Path;
        if (PART_ArrowPath != null)
        {
            PART_ArrowPath.Data = PathOpen;
            PART_ArrowPath.GotFocus -= PART_ArrowPath_GotFocus;
            PART_ArrowPath.GotFocus += PART_ArrowPath_GotFocus;
            PART_ArrowPath.LostFocus -= PART_ArrowPath_LostFocus;
            PART_ArrowPath.LostFocus += PART_ArrowPath_LostFocus;
            PART_ArrowPath.PreviewKeyDown -= PART_ArrowPath_PreviewKeyDown;
            PART_ArrowPath.PreviewKeyDown += PART_ArrowPath_PreviewKeyDown;
        }

        PART_Popup = GetTemplateChild(nameof(PART_Popup)) as Popup;
        if (PART_Popup != null)
        {
            SetPlacement();
            PART_Popup.Closed -= PART_Popup_Closed;
            PART_Popup.Closed += PART_Popup_Closed;
        }

        PART_CloseButton = GetTemplateChild(nameof(PART_CloseButton)) as WpfButton;
        if (PART_CloseButton != null)
        {
            PART_CloseButton.Click -= PART_CloseButton_Click;
            PART_CloseButton.Click += PART_CloseButton_Click;
        }
    }

    public WpfFlyoutPopup()
    {
        DataContext = this;
        Loaded -= WpfFlyoutOverlay_Loaded;
        Loaded += WpfFlyoutOverlay_Loaded;
    }

    private async void WpfFlyoutOverlay_Loaded(object sender, RoutedEventArgs e)
    {
        Header2 ??= Header;
        await HandleFlyArrowAsync(true);
    }

    private static CustomPopupPlacement[] PopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
    {
        CustomPopupPlacement[] customPopupPlacements = Array.Empty<CustomPopupPlacement>();
        try
        {
            double x = (targetSize.Width - popupSize.Width) / 2;
            double yOffset = 100;
            double yCenter = ((targetSize.Height - popupSize.Height) / 2);
            double y = yCenter > yOffset ? yOffset : yCenter;
            Point point = new(x, y);
            customPopupPlacements = new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.Horizontal) };
        }
        catch (Exception) { }
        return customPopupPlacements;
    }

    private void SetPlacement()
    {
        try
        {
            if (PART_HeaderStackPanel == null) return;
            if (PART_Popup == null) return;
            if (Placement == PlacementMode.Center)
            {
                if (ParentWindow is UIElement parentWindowUIElement)
                {
                    PART_Popup.PlacementTarget = parentWindowUIElement;
                    PART_Popup.Placement = PlacementMode.Custom;
                    PART_Popup.CustomPopupPlacementCallback = PopupPlacementCallback;
                }
            }
            else
            {
                PART_Popup.PlacementTarget = PART_HeaderStackPanel;
                PART_Popup.Placement = Placement;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WpfFlyoutOverlay SetPlacement: " + ex.Message);
        }
    }

    private int GetToOpenDegree()
    {
        int degree = 0;
        if (Placement == PlacementMode.Left) degree = 45;
        else if (Placement == PlacementMode.Top) degree = -180;
        else if (Placement == PlacementMode.Right) degree = -45;
        return degree;
    }

    private bool IsHandling = false;

    private async Task HandleFlyArrowAsync(bool anyway)
    {
        try
        {
            if (PART_Popup == null) return;
            if (PART_ArrowPath == null) return;

            if (IsHandling) return;
            IsHandling = true;

            bool isOpen = PART_Popup.IsOpen;

            if (isOpen != IsOpen || anyway)
            {
                if (isOpen)
                {
                    if (!anyway) FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(true));
                    await PART_ArrowPath.AnimRotateAsync(GetToOpenDegree(), AnimDurationMS); // -90, 0
                }
                else
                {
                    if (!anyway) FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(false));
                    await PART_ArrowPath.AnimRotateAsync(-90, AnimDurationMS); // 0, -90
                }
                IsOpen = isOpen;
            }

            IsHandling = false;

            // After Anim
            isOpen = PART_Popup.IsOpen;
            if (isOpen != IsOpen) await HandleFlyArrowAsync(false);
        }
        catch (Exception) { }
    }

    private bool IsOpenning = false;

    public async Task OpenFlyAsync()
    {
        try
        {
            if (PART_Popup == null) return;

            if (IsOpenning) return;
            IsOpenning = true;

            bool isOpen = PART_Popup.IsOpen;
            if (!isOpen)
            {
                PART_Popup.IsOpen = true;
                
                // Add Handler: Capture Mouse Click Outside Of The Element
                if (!StaysOpen)
                {
                    Mouse.Capture(this, CaptureMode.SubTree);
                    AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(HandleClickOutsideOfControl), true);
                }
                
                await Task.Delay(10);
                await HandleFlyArrowAsync(false);

                PART_Popup.StaysOpen = StaysOpen;
            }

            IsOpenning = false;
        }
        catch (Exception) { }
    }

    private bool IsClosing = false;

    public async Task CloseFlyAsync()
    {
        try
        {
            if (PART_Popup == null) return;

            if (IsClosing) return;
            IsClosing = true;

            bool isOpen = PART_Popup.IsOpen;
            if (isOpen)
            {
                PART_Popup.IsOpen = false;

                // Remove Handler And Release Mouse Capture
                if (!StaysOpen)
                {
                    RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(HandleClickOutsideOfControl));
                    ReleaseMouseCapture();
                }
                
                await Task.Delay(10);
                await HandleFlyArrowAsync(false);

                PART_Popup.StaysOpen = true; // Set To Default
            }

            IsClosing = false;
        }
        catch (Exception) { }
    }

    private async void HandleClickOutsideOfControl(object sender, MouseButtonEventArgs e)
    {
        await CloseFlyAsync();
    }

    public async void ToggleFly()
    {
        try
        {
            if (PART_Popup == null) return;
            if (IsOpenning || IsClosing) return;
            bool isOpen = PART_Popup.IsOpen;
            if (isOpen) await CloseFlyAsync();
            else await OpenFlyAsync();
        }
        catch (Exception) { }
    }

    private void RedrawPopup()
    {
        try
        {
            if (PART_Popup == null) return;
            if (PART_Popup.IsOpen)
            {
                double offset = PART_Popup.HorizontalOffset;
                PART_Popup.HorizontalOffset = offset + 1;
                PART_Popup.HorizontalOffset = offset;
            }
        }
        catch (Exception) { }
    }

    private void ParentWindow_Activated(object? sender, EventArgs e)
    {
        try
        {
            if (PART_Popup == null) return;
            bool isOpen = PART_Popup.IsOpen;
            if (IsOpen && !isOpen)
            {
                PART_Popup.IsOpen = true;
            }
        }
        catch (Exception) { }
    }

    private void ParentWindow_Deactivated(object? sender, EventArgs e)
    {
        try
        {
            if (PART_Popup == null) return;
            bool isOpen = PART_Popup.IsOpen;
            if (isOpen)
            {
                PART_Popup.IsOpen = false;
            }
        }
        catch (Exception) { }
    }

    private void ParentWindow_LocationChanged(object? sender, EventArgs e)
    {
        RedrawPopup();
    }

    private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RedrawPopup();
    }

    private void PART_HeaderStackPanel_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ToggleFly();
    }

    private async void PART_HeaderStackPanel_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (PART_HeaderStackPanel == null) return;
        if (!PART_HeaderStackPanel.IsEnabled) await CloseFlyAsync();
    }

    private void PART_ArrowPath_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_CircleEllipse == null) return;
            if (PART_ArrowPath == null) return;
            Brush brush = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);
            PART_CircleEllipse.Stroke = brush;
            PART_ArrowPath.Fill = brush;
        }
        catch (Exception) { }
    }

    private void PART_ArrowPath_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_CircleEllipse == null) return;
            if (PART_ArrowPath == null) return;
            PART_CircleEllipse.Stroke = BorderBrush;
            PART_ArrowPath.Fill = BorderBrush;
        }
        catch (Exception) { }
    }

    private void PART_ArrowPath_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Space) ToggleFly();
    }

    private async void PART_Popup_Closed(object? sender, EventArgs e)
    {
        if (IsOpen) await HandleFlyArrowAsync(false);
    }

    private async void PART_CloseButton_Click(object sender, RoutedEventArgs e)
    {
        await CloseFlyAsync();
    }

}