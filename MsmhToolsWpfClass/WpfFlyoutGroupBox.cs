using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

public class WpfFlyoutGroupBox : GroupBox
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
        DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(WpfFlyoutGroupBox),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrushTransparent)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfFlyoutGroupBox),
            new PropertyMetadata(new CornerRadius(5)));

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

    [Category("Common")]
    public bool CloseFly
    {
        get { return (bool)GetValue(CloseFlyProperty); }
        set { SetValue(CloseFlyProperty, value); }
    }
    public static readonly DependencyProperty CloseFlyProperty =
        DependencyProperty.Register(nameof(CloseFly), typeof(bool), typeof(WpfFlyoutGroupBox), new PropertyMetadata(false));

    [Category("Common")]
    public bool DisableFly
    {
        get { return (bool)GetValue(DisableFlyProperty); }
        set { SetValue(DisableFlyProperty, value); }
    }
    public static readonly DependencyProperty DisableFlyProperty =
        DependencyProperty.Register(nameof(DisableFly), typeof(bool), typeof(WpfFlyoutGroupBox), new PropertyMetadata(false));

    [Category("Common")]
    public bool HideHeader
    {
        get { return (bool)GetValue(HideHeaderProperty); }
        set { SetValue(HideHeaderProperty, value); }
    }
    public static readonly DependencyProperty HideHeaderProperty =
        DependencyProperty.Register(nameof(HideHeader), typeof(bool), typeof(WpfFlyoutGroupBox), new PropertyMetadata(false));

    public int AnimDurationMS { get; set; } = 200;

    public bool IsOpen { get; private set; }

    private Grid? PART_MainGrid;
    private StackPanel? PART_HeaderStackPanel;
    private Path? PART_ArrowPath;
    private Grid? PART_ContentGrid;

    private readonly Geometry PathOpen = Geometry.Parse("M 5 1 L 5 9 L 7 7 L 7 8 L 4 12 L 1 8 L 1 7 L 3 9 L 3 1 L 5 1 Z");
    //private readonly Geometry PathClose = Geometry.Parse("M 0 3 L 8 3 L 6 1 L 7 1 L 11 4 L 7 7 L 6 7 L 8 5 L 0 5 L 0 3 Z");

    public override void OnApplyTemplate()
    {
        PART_MainGrid = GetTemplateChild(nameof(PART_MainGrid)) as Grid;
        // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!
        PART_MainGrid?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, this);

        PART_HeaderStackPanel = GetTemplateChild(nameof(PART_HeaderStackPanel)) as StackPanel;
        if (PART_HeaderStackPanel != null)
        {
            PART_HeaderStackPanel.PreviewMouseLeftButtonDown -= PART_HeaderStackPanel_PreviewMouseLeftButtonDown;
            PART_HeaderStackPanel.PreviewMouseLeftButtonDown += PART_HeaderStackPanel_PreviewMouseLeftButtonDown;

            if (HideHeader) PART_HeaderStackPanel.Visibility = Visibility.Collapsed;
        }
        
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

        PART_ContentGrid = GetTemplateChild(nameof(PART_ContentGrid)) as Grid;

        base.OnApplyTemplate();
    }

    public WpfFlyoutGroupBox()
    {
        DataContext = this;
        SizeChanged -= WpfFlyoutGroupBox_SizeChanged;
        SizeChanged += WpfFlyoutGroupBox_SizeChanged;
        Loaded -= WpfFlyoutGroupBox_Loaded;
        Loaded += WpfFlyoutGroupBox_Loaded;
    }

    private double FlyElementActualH = double.NaN;

    private double GetFlyElementActualHeight()
    {
        if (PART_HeaderStackPanel == null) return 0;
        if (PART_ContentGrid == null) return 0;
        return ActualHeight - Margin.Top - Margin.Bottom - PART_HeaderStackPanel.ActualHeight - PART_HeaderStackPanel.Margin.Top - PART_HeaderStackPanel.Margin.Bottom - PART_ContentGrid.Margin.Top - PART_ContentGrid.Margin.Bottom;
    }

    private bool IsHandling = false;
    private int LastDegree = 0;

    private async Task HandleFlyArrowAsync(bool anyway)
    {
        try
        {
            if (PART_ContentGrid == null) return;
            if (PART_ArrowPath == null) return;

            if (IsHandling) return;
            IsHandling = true;

            bool isOpen = PART_ContentGrid.ActualHeight > 0;

            if (isOpen != IsOpen || anyway)
            {
                if (isOpen)
                {
                    FlyElementActualH = GetFlyElementActualHeight();
                    PART_ContentGrid.IsEnabled = true;
                    await PART_ArrowPath.RotateAsync(LastDegree, 0, AnimDurationMS); // -90, 0
                    LastDegree = 0;
                    if (!anyway && !CloseFly) FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(true));
                }
                else
                {
                    await PART_ArrowPath.RotateAsync(LastDegree, -90, AnimDurationMS); // 0, -90
                    LastDegree = -90;
                    PART_ContentGrid.IsEnabled = false;
                    if (!anyway) FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(false));
                }
                IsOpen = isOpen;
            }

            IsHandling = false;

            // After Anim
            isOpen = PART_ContentGrid.ActualHeight > 0;
            if (isOpen != IsOpen) await HandleFlyArrowAsync(false);
        }
        catch (Exception) { }
    }

    private bool IsOpenning = false;

    public async Task OpenFlyAsync()
    {
        try
        {
            if (DisableFly) return;
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;
            if (PART_ContentGrid == null) return;

            if (IsOpenning) return;
            IsOpenning = true;

            bool isOpen = PART_ContentGrid.ActualHeight > 0;
            if (!isOpen)
            {
                if (CloseFly) CloseFly = false;

                PART_ContentGrid.Margin = new Thickness(2);

                if (double.IsNaN(FlyElementActualH) || ActualHeight > PART_HeaderStackPanel.ActualHeight * 2)
                    FlyElementActualH = GetFlyElementActualHeight();
                DoubleAnimation anim = new(0, FlyElementActualH, new Duration(TimeSpan.FromMilliseconds(AnimDurationMS)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);

                await Task.Delay(AnimDurationMS);

                if (double.IsNaN(FlyElementActualH) || ActualHeight > PART_HeaderStackPanel.ActualHeight * 2)
                    FlyElementActualH = GetFlyElementActualHeight();
                anim = new(FlyElementActualH, double.MaxValue, new Duration(TimeSpan.FromMilliseconds(10)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);
                PART_ContentGrid.MaxHeight = double.MaxValue;

                await Task.Delay(10);

                await HandleFlyArrowAsync(false); // For Middle Position
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
            if (DisableFly) return;
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;
            if (PART_ContentGrid == null) return;

            if (IsClosing) return;
            IsClosing = true;

            bool isOpen = PART_ContentGrid.ActualHeight > 0;
            if (isOpen)
            {
                FlyElementActualH = GetFlyElementActualHeight();
                DoubleAnimation anim = new(FlyElementActualH, 0, new Duration(TimeSpan.FromMilliseconds(AnimDurationMS)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);

                await Task.Delay(AnimDurationMS);

                PART_ContentGrid.Margin = new Thickness(0);
                await Task.Delay(10);
                await HandleFlyArrowAsync(false); // For Middle Position
            }

            IsClosing = false;
        }
        catch (Exception) { }
    }

    public async void ToggleFly()
    {
        try
        {
            if (DisableFly) return;
            if (PART_ContentGrid == null) return;
            if (IsOpenning || IsClosing) return;
            bool isOpen = PART_ContentGrid.ActualHeight > 0;
            if (isOpen) await CloseFlyAsync();
            else await OpenFlyAsync();
        }
        catch (Exception) { }
    }

    private async void WpfFlyoutGroupBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        await HandleFlyArrowAsync(false);
    }

    private async void WpfFlyoutGroupBox_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;

            if (CloseFly) await CloseFlyAsync();

            if (DisableFly)
            {
                PART_HeaderStackPanel.Cursor = Cursors.Arrow;
                PART_ArrowPath.Focusable = false;
            }
            else
            {
                PART_HeaderStackPanel.Cursor = Cursors.Hand;
                PART_ArrowPath.Focusable = true;
            }

            if (HideHeader) PART_HeaderStackPanel.Visibility = Visibility.Collapsed;

            await HandleFlyArrowAsync(true);
        }
        catch (Exception) { }
    }

    private void PART_HeaderStackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ToggleFly();
    }

    private void PART_ArrowPath_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_ArrowPath == null) return;
            PART_ArrowPath.Fill = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);
        }
        catch (Exception) { }
    }

    private void PART_ArrowPath_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_ArrowPath == null) return;
            PART_ArrowPath.Fill = Foreground;
        }
        catch (Exception) { }
    }

    private void PART_ArrowPath_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Space) ToggleFly();
    }

}