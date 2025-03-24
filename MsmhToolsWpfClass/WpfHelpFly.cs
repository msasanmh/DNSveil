using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MsmhToolsWpfClass;

public class WpfHelpFly : GroupBox
{
    [Category("Brush")]
    public Brush HeaderBrush
    {
        get { return (Brush)GetValue(HeaderBrushProperty); }
        set { SetValue(HeaderBrushProperty, value); }
    }
    public static readonly DependencyProperty HeaderBrushProperty =
        DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(WpfHelpFly),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrushTransparent)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfHelpFly),
            new PropertyMetadata(new CornerRadius(5)));

    [Category("Common")]
    public PlacementMode Placement
    {
        get { return (PlacementMode)GetValue(PlacementProperty); }
        set { SetValue(PlacementProperty, value); }
    }
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(WpfHelpFly),
            new PropertyMetadata(PlacementMode.Bottom, (s, e) =>
            {
                if (s is WpfHelpFly wpfHelpFly)
                {
                    wpfHelpFly.SetPlacement();
                }
            }));

    public int AnimDurationMS { get; set; } = 200;

    private Window? ParentWindow;
    private Grid? PART_HeaderGrid;
    private Popup? PART_Popup;
    private WpfButton? PART_CloseButton;

    public override void OnApplyTemplate()
    {
        ParentWindow = this.GetParentWindow();
        if (ParentWindow != null)
        {
            ParentWindow.Deactivated -= ParentWindow_Deactivated;
            ParentWindow.Deactivated += ParentWindow_Deactivated;
            ParentWindow.LocationChanged -= ParentWindow_LocationChanged;
            ParentWindow.LocationChanged += ParentWindow_LocationChanged;
            ParentWindow.SizeChanged -= ParentWindow_SizeChanged;
            ParentWindow.SizeChanged += ParentWindow_SizeChanged;
        }

        PART_HeaderGrid = GetTemplateChild(nameof(PART_HeaderGrid)) as Grid;
        if (PART_HeaderGrid != null)
        {
            PART_HeaderGrid.PreviewMouseLeftButtonDown -= PART_HeaderGrid_PreviewMouseLeftButtonDown;
            PART_HeaderGrid.PreviewMouseLeftButtonDown += PART_HeaderGrid_PreviewMouseLeftButtonDown;
            PART_HeaderGrid.IsEnabledChanged -= PART_HeaderGrid_IsEnabledChanged;
            PART_HeaderGrid.IsEnabledChanged += PART_HeaderGrid_IsEnabledChanged;
        }

        PART_Popup = GetTemplateChild(nameof(PART_Popup)) as Popup;
        if (PART_Popup != null)
        {
            SetPlacement();
        }

        PART_CloseButton = GetTemplateChild(nameof(PART_CloseButton)) as WpfButton;
        if (PART_CloseButton != null)
        {
            PART_CloseButton.Click -= PART_CloseButton_Click;
            PART_CloseButton.Click += PART_CloseButton_Click;
        }

        base.OnApplyTemplate();
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
            if (PART_HeaderGrid == null) return;
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
                PART_Popup.PlacementTarget = PART_HeaderGrid;
                PART_Popup.Placement = Placement;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WpfHelpFly SetPlacement: " + ex.Message);
        }
    }

    public WpfHelpFly()
    {
        DataContext = this;
        Focusable = false;
        Header ??= "Help";
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
                Mouse.Capture(this, CaptureMode.SubTree);
                AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(HandleClickOutsideOfControl), true);

                await Task.Delay(10);
                PART_Popup.StaysOpen = false;
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
                RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(HandleClickOutsideOfControl));
                ReleaseMouseCapture();

                await Task.Delay(10);
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

    private async void ParentWindow_Deactivated(object? sender, EventArgs e)
    {
        await CloseFlyAsync();
    }

    private async void ParentWindow_LocationChanged(object? sender, EventArgs e)
    {
        await CloseFlyAsync();
    }

    private async void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        await CloseFlyAsync();
    }

    private void PART_HeaderGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ToggleFly();
    }

    private async void PART_HeaderGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (PART_HeaderGrid == null) return;
        if (!PART_HeaderGrid.IsEnabled) await CloseFlyAsync();
    }

    private async void PART_CloseButton_Click(object sender, RoutedEventArgs e)
    {
        await CloseFlyAsync();
    }

}