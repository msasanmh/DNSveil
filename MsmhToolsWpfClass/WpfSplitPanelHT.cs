using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MsmhToolsClass;
using MsmhToolsWpfClass.Themes;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;

namespace MsmhToolsWpfClass;

public class WpfSplitPanelHT : Control
{
    [Category("Brush")]
    public Brush SplitterBrush
    {
        get { return (Brush)GetValue(SplitterBrushProperty); }
        set { SetValue(SplitterBrushProperty, value); }
    }
    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(WpfSplitPanelHT),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrush)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfSplitPanelHT),
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
    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(WpfSplitPanelHT), new PropertyMetadata(nameof(WpfSplitPanelHT)));

    [Category("Common")]
    public object Panel1
    {
        get { return GetValue(Panel1Property); }
        set { SetValue(Panel1Property, value); }
    }
    public static readonly DependencyProperty Panel1Property =
        DependencyProperty.Register(nameof(Panel1), typeof(object), typeof(WpfSplitPanelHT), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public object Panel2
    {
        get { return GetValue(Panel2Property); }
        set { SetValue(Panel2Property, value); }
    }
    public static readonly DependencyProperty Panel2Property =
        DependencyProperty.Register(nameof(Panel2), typeof(object), typeof(WpfSplitPanelHT), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public bool FixedPanel
    {
        get { return (bool)GetValue(FixedPanelProperty); }
        set { SetValue(FixedPanelProperty, value); }
    }
    public static readonly DependencyProperty FixedPanelProperty =
        DependencyProperty.Register(nameof(FixedPanel), typeof(bool), typeof(WpfSplitPanelHT), new PropertyMetadata(false));

    [Category("Common")]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(WpfSplitPanelHT),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static async void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfSplitPanelHT panelHT)
        {
            await panelHT.UpdatePositionAsync(true);
        }
    }

    private int Panel1Percent
    {
        get { return (int)GetValue(Panel1PercentProperty); }
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            SetValue(Panel1PercentProperty, value);
        }
    }
    private static readonly DependencyProperty Panel1PercentProperty =
        DependencyProperty.Register(nameof(Panel1Percent), typeof(int), typeof(WpfSplitPanelHT),
            new PropertyMetadata(30));

    private Border? PART_RootBorder;
    private Grid? PART_MainGrid;
    private GridSplitter? PART_GridSplitter;
    private Border? PART_SplitterBorder;
    private Ellipse? PART_Ellipse;
    private Path? PART_Path;
    private TextBlock? PART_TextBlock;
    private Grid? PART_Content1Grid;
    private Grid? PART_Content2Grid;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_RootBorder = GetTemplateChild(nameof(PART_RootBorder)) as Border;
        PART_RootBorder?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_RootBorder); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

        PART_MainGrid = GetTemplateChild(nameof(PART_MainGrid)) as Grid;
        PART_MainGrid?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_MainGrid); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

        PART_GridSplitter = GetTemplateChild(nameof(PART_GridSplitter)) as GridSplitter;
        if (PART_GridSplitter != null)
        {
            PART_GridSplitter.IsHitTestVisible = !FixedPanel;
        }

        PART_SplitterBorder = GetTemplateChild(nameof(PART_SplitterBorder)) as Border;
        if (PART_SplitterBorder != null)
        {
            PART_SplitterBorder.IsHitTestVisible = !FixedPanel;
            PART_SplitterBorder.PreviewMouseLeftButtonDown -= PART_SplitterBorder_PreviewMouseLeftButtonDown;
            PART_SplitterBorder.PreviewMouseLeftButtonDown += PART_SplitterBorder_PreviewMouseLeftButtonDown;
            PART_SplitterBorder.GotFocus -= PART_SplitterBorder_GotFocus;
            PART_SplitterBorder.GotFocus += PART_SplitterBorder_GotFocus;
            PART_SplitterBorder.LostFocus -= PART_SplitterBorder_LostFocus;
            PART_SplitterBorder.LostFocus += PART_SplitterBorder_LostFocus;
            PART_SplitterBorder.PreviewKeyDown -= PART_SplitterBorder_PreviewKeyDown;
            PART_SplitterBorder.PreviewKeyDown += PART_SplitterBorder_PreviewKeyDown;
        }

        PART_Ellipse = GetTemplateChild(nameof(PART_Ellipse)) as Ellipse;
        PART_Path = GetTemplateChild(nameof(PART_Path)) as Path;
        PART_TextBlock = GetTemplateChild(nameof(PART_TextBlock)) as TextBlock;

        if (PART_Ellipse != null && PART_Path != null && FixedPanel)
        {
            PART_Ellipse.Visibility = Visibility.Hidden;
            PART_Path.Visibility = Visibility.Hidden;
        }

        PART_Content1Grid = GetTemplateChild(nameof(PART_Content1Grid)) as Grid;
        PART_Content2Grid = GetTemplateChild(nameof(PART_Content2Grid)) as Grid;
    }

    public WpfSplitPanelHT()
    {
        DataContext = this;
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            SplitterBrush = Brushes.DodgerBlue;
        }

        Loaded -= WpfSplitPanelHT_Loaded;
        Loaded += WpfSplitPanelHT_Loaded;
    }

    private async void WpfSplitPanelHT_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_MainGrid == null) return;
            if (PART_GridSplitter == null) return;
            if (PART_SplitterBorder == null) return;
            if (PART_Ellipse == null) return;
            if (PART_Path == null) return;
            if (PART_TextBlock == null) return;

            double eX = 20; // Left Offset
            double eY = 0 - (PART_Ellipse.ActualHeight / 2) + (PART_SplitterBorder.ActualHeight / 2);
            PART_Ellipse.SetValue(Canvas.LeftProperty, eX);
            PART_Ellipse.SetValue(Canvas.TopProperty, eY);

            PART_Path.SetValue(Canvas.LeftProperty, eX + 3); // Plus The Width Subtract Of Ellipse / 2
            PART_Path.SetValue(Canvas.TopProperty, eY + 3); // Plus The Width Subtract Of Ellipse / 2

            PART_TextBlock.SetValue(Canvas.LeftProperty, eX + 30);
            PART_TextBlock.SetValue(Canvas.TopProperty, eY + 7);

            // Wait For Auto Size
            PART_MainGrid.Visibility = Visibility.Hidden;
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    double gl1 = 0;
                    PART_MainGrid.DispatchIt(() => gl1 = PART_MainGrid.RowDefinitions[0].ActualHeight);
                    await Task.Delay(50);
                    double gl2 = 0;
                    PART_MainGrid.DispatchIt(() => gl2 = PART_MainGrid.RowDefinitions[0].ActualHeight);
                    if (gl1 == gl2) break;
                    await Task.Delay(5);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromMilliseconds(2000)); } catch (Exception) { }
            PART_MainGrid.Visibility = Visibility.Visible;

            await UpdatePositionAsync(false);
        }
        catch (Exception) { }
    }

    private int GetCurrentPanel1Percent()
    {
        int p1Percent = 0;
        if (PART_MainGrid != null)
        {
            try
            {
                double p1 = PART_MainGrid.RowDefinitions[0].ActualHeight;
                double p2 = PART_MainGrid.RowDefinitions[2].ActualHeight;
                double sum = p1 + p2;
                if (sum < 1) sum = 1;
                p1Percent = (p1 * 100 / sum).ToInt();

                if (p1Percent > 0 && p1Percent < 100) Panel1Percent = p1Percent;
            }
            catch (Exception) { }
        }
        return p1Percent;
    }

    private enum ToggleState
    {
        Top, Middle, Bottom, Unknown
    }

    private ToggleState GetToggleStatus()
    {
        ToggleState result = ToggleState.Unknown;
        if (PART_Content1Grid == null || PART_Content2Grid == null) return result;
        if (PART_Content1Grid.ActualHeight > 0 && PART_Content2Grid.ActualHeight == 0) result = ToggleState.Bottom;
        if (PART_Content1Grid.ActualHeight == 0 && PART_Content2Grid.ActualHeight > 0) result = ToggleState.Top;
        if (PART_Content1Grid.ActualHeight > 0 && PART_Content2Grid.ActualHeight > 0)
        {
            int p1Percent = GetCurrentPanel1Percent();
            if (Panel1Percent == p1Percent) result = ToggleState.Middle;
        }
        return result;
    }

    private async Task ToMiddleAsync()
    {
        try
        {
            if (PART_MainGrid == null) return;
            int p1Percent = GetCurrentPanel1Percent();
            if (Panel1Percent > p1Percent)
            {
                int animSpeed = 0;
                if (Panel1Percent - p1Percent >= 40) animSpeed = 2;
                if (Panel1Percent - p1Percent >= 60) animSpeed = 3;
                int LastN = 0;
                for (int n = p1Percent; n <= Panel1Percent; n++)
                {
                    if (animSpeed > 0 && n > p1Percent + 10 && n < Panel1Percent - 10) n += animSpeed;
                    PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                    LastN = n;
                    if (n == Panel1Percent) break;
                    await Task.Delay(1);
                }

                if (LastN == Panel1Percent)
                {
                    double n = LastN + 0.5;
                    PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                }
            }
            else if (Panel1Percent < p1Percent)
            {
                int animSpeed = 0;
                if (p1Percent - Panel1Percent >= 40) animSpeed = 2;
                if (p1Percent - Panel1Percent >= 60) animSpeed = 3;
                for (int n = p1Percent; n >= Panel1Percent; n--)
                {
                    if (animSpeed > 0 && n < p1Percent - 10 && n > Panel1Percent + 10) n -= animSpeed;
                    PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                    if (n == Panel1Percent) break;
                    await Task.Delay(1);
                }
            }

            PART_MainGrid.RowDefinitions[0].Height = GridLength.Auto;
        }
        catch (Exception) { }
    }

    private async Task MiddleToTopAsync()
    {
        try
        {
            if (PART_MainGrid == null) return;
            int animSpeed = 0;
            if (Panel1Percent >= 40) animSpeed = 2;
            if (Panel1Percent >= 60) animSpeed = 3;
            for (int n = Panel1Percent; n >= 0; n--)
            {
                if (animSpeed > 0 && n < Panel1Percent - 10 && n > 10) n -= animSpeed;
                PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                await Task.Delay(1);
            }

            if (DesignerProperties.GetIsInDesignMode(this))
                PART_MainGrid.RowDefinitions[0].Height = new GridLength(0);
        }
        catch (Exception) { }
    }

    private async Task BottomToTopAsync()
    {
        try
        {
            if (PART_MainGrid == null) return;
            int animSpeed = 3;
            for (int n = 0; n <= 100; n++)
            {
                if (animSpeed > 0 && n > 10 && n < 90) n += animSpeed;
                PART_MainGrid.RowDefinitions[0].Height = new GridLength(100 - n, GridUnitType.Star);
                PART_MainGrid.RowDefinitions[2].Height = new GridLength(n, GridUnitType.Star);
                await Task.Delay(1);
            }

            if (DesignerProperties.GetIsInDesignMode(this))
                PART_MainGrid.RowDefinitions[0].Height = new GridLength(0);
        }
        catch (Exception) { }
    }

    private bool IsHandling = false;

    private async Task UpdatePositionAsync(bool animate)
    {
        try
        {
            if (PART_MainGrid == null) return;
            if (PART_Content1Grid == null) return;
            if (IsHandling) return;
            IsHandling = true;
            ToggleState toggleState = GetToggleStatus();
            if (animate)
            {
                if (IsOpen)
                {
                    // To Middle
                    await ToMiddleAsync();
                }
                else
                {
                    if (toggleState == ToggleState.Unknown)
                    {
                        // Middle To Top
                        await MiddleToTopAsync();
                    }
                    else if (toggleState == ToggleState.Middle)
                    {
                        // Middle To Top
                        await MiddleToTopAsync();
                    }
                    else if (toggleState == ToggleState.Bottom)
                    {
                        // Right To Left
                        await BottomToTopAsync();
                    }
                }
            }
            else
            {
                if (IsOpen)
                {
                    PART_MainGrid.RowDefinitions[0].Height = GridLength.Auto;
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    PART_MainGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(100, GridUnitType.Star);

                    if (DesignerProperties.GetIsInDesignMode(this))
                        PART_MainGrid.RowDefinitions[0].Height = new GridLength(0);
                }
            }
            IsHandling = false;
        }
        catch (Exception) { }
    }

    private void PART_SplitterBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            IsOpen = !IsOpen;
            PART_SplitterBorder?.Focus();
        }
        catch (Exception) { }
    }

    private void PART_SplitterBorder_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_Ellipse == null) return;
            PART_Ellipse.Stroke = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);
        }
        catch (Exception) { }
    }

    private void PART_SplitterBorder_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PART_Ellipse == null) return;
            PART_Ellipse.Stroke = Foreground;
        }
        catch (Exception) { }
    }

    private void PART_SplitterBorder_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Space) IsOpen = !IsOpen;
    }

}