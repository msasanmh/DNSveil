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

    [Category("Common")]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(WpfFlyoutGroupBox),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static async void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfFlyoutGroupBox box)
        {
            await box.UpdatePositionAsync();
        }
    }

    public int AnimDurationMS { get; set; } = 300;

    private Grid? PART_MainGrid;
    private StackPanel? PART_HeaderStackPanel;
    private Path? PART_ArrowPath;
    private Grid? PART_ContentGrid;

    private readonly Geometry PathOpen = Geometry.Parse("M 5 1 L 5 9 L 7 7 L 7 8 L 4 12 L 1 8 L 1 7 L 3 9 L 3 1 L 5 1 Z");
    //private readonly Geometry PathClose = Geometry.Parse("M 0 3 L 8 3 L 6 1 L 7 1 L 11 4 L 7 7 L 6 7 L 8 5 L 0 5 L 0 3 Z");

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

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
    }

    public WpfFlyoutGroupBox()
    {
        DataContext = this;
        Loaded += WpfFlyoutGroupBox_Loaded;
        Loaded += WpfFlyoutGroupBox_Loaded;
    }

    private async void WpfFlyoutGroupBox_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_MainGrid == null) return;
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;
            if (PART_ContentGrid == null) return;

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

            // Wait For Auto Size
            PART_MainGrid.Visibility = Visibility.Hidden;
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    double ah1 = 0;
                    PART_ContentGrid.DispatchIt(() => ah1 = PART_ContentGrid.ActualHeight);
                    await Task.Delay(50);
                    double ah2 = 0;
                    PART_ContentGrid.DispatchIt(() => ah2 = PART_ContentGrid.ActualHeight);
                    if (ah1 == ah2) break;
                    await Task.Delay(5);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromMilliseconds(2000)); } catch (Exception) { }
            PART_MainGrid.Visibility = Visibility.Visible;

            await UpdatePositionAsync();
        }
        catch (Exception) { }
    }

    private double FlyElementActualH = double.NaN;

    private double GetFlyElementActualHeight()
    {
        if (PART_HeaderStackPanel == null) return 0;
        if (PART_ContentGrid == null) return 0;
        return ActualHeight - Margin.Top - Margin.Bottom - PART_HeaderStackPanel.ActualHeight - PART_HeaderStackPanel.Margin.Top - PART_HeaderStackPanel.Margin.Bottom - PART_ContentGrid.Margin.Top - PART_ContentGrid.Margin.Bottom;
    }

    private bool IsOpenning = false;

    private async Task OpenFlyAsync()
    {
        try
        {
            if (DisableFly) return;
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;
            if (PART_ContentGrid == null) return;

            if (IsOpenning || IsClosing) return;
            IsOpenning = true;

            bool isOpen = PART_ContentGrid.ActualHeight > 1;
            if (!isOpen)
            {
                FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(true));

                if (double.IsNaN(FlyElementActualH) || ActualHeight > PART_HeaderStackPanel.ActualHeight * 2)
                    FlyElementActualH = GetFlyElementActualHeight();
                DoubleAnimation anim = new(0, FlyElementActualH, new Duration(TimeSpan.FromMilliseconds(AnimDurationMS)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);

                // Handle Fly Arrow
                await PART_ArrowPath.AnimRotateAsync(0, AnimDurationMS); // -90, 0
                PART_ContentGrid.IsEnabled = true;

                if (double.IsNaN(FlyElementActualH) || ActualHeight > PART_HeaderStackPanel.ActualHeight * 2)
                    FlyElementActualH = GetFlyElementActualHeight();
                anim = new(FlyElementActualH, double.MaxValue, new Duration(TimeSpan.FromMilliseconds(10)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);
                PART_ContentGrid.MaxHeight = double.MaxValue;

                await Task.Delay(10);
            }

            IsOpenning = false;
        }
        catch (Exception) { }
    }

    private bool IsClosing = false;

    private async Task CloseFlyAsync()
    {
        try
        {
            if (DisableFly) return;
            if (PART_HeaderStackPanel == null) return;
            if (PART_ArrowPath == null) return;
            if (PART_ContentGrid == null) return;

            if (IsOpenning || IsClosing) return;
            IsClosing = true;

            bool isOpen = PART_ContentGrid.ActualHeight > 1;
            if (isOpen)
            {
                FlyoutChanged?.Invoke(this, new FlyoutChangedEventArgs(false));

                FlyElementActualH = GetFlyElementActualHeight();
                DoubleAnimation anim = new(FlyElementActualH, 0, new Duration(TimeSpan.FromMilliseconds(AnimDurationMS)));
                PART_ContentGrid.BeginAnimation(MaxHeightProperty, anim);

                // Handle Fly Arrow
                await PART_ArrowPath.AnimRotateAsync(-90, AnimDurationMS); // 0, -90
                PART_ContentGrid.IsEnabled = false;

                
                await Task.Delay(10);
            }

            IsClosing = false;
        }
        catch (Exception) { }
    }

    private async Task UpdatePositionAsync()
    {
        try
        {
            if (DisableFly) return;
            if (PART_ContentGrid == null) return;
            if (IsOpenning || IsClosing) return;
            if (IsOpen) await OpenFlyAsync();
            else await CloseFlyAsync();
        }
        catch (Exception) { }
    }

    private void PART_HeaderStackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        IsOpen = !IsOpen;
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
        if (e.Key == Key.Enter || e.Key == Key.Space) IsOpen = !IsOpen;
    }

}