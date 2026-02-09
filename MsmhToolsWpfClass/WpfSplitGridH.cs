using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MsmhToolsClass;
using MsmhToolsWpfClass.Themes;

namespace MsmhToolsWpfClass;

public class WpfSplitGridH : Control
{
    [Category("Brush")]
    public Brush SplitterBrush
    {
        get { return (Brush)GetValue(SplitterBrushProperty); }
        set { SetValue(SplitterBrushProperty, value); }
    }
    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(WpfSplitGridH),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrush)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfSplitGridH),
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
    public object Panel1
    {
        get { return GetValue(Panel1Property); }
        set { SetValue(Panel1Property, value); }
    }
    public static readonly DependencyProperty Panel1Property =
        DependencyProperty.Register(nameof(Panel1), typeof(object), typeof(WpfSplitGridH), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public object Panel2
    {
        get { return GetValue(Panel2Property); }
        set { SetValue(Panel2Property, value); }
    }
    public static readonly DependencyProperty Panel2Property =
        DependencyProperty.Register(nameof(Panel2), typeof(object), typeof(WpfSplitGridH), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public int Panel1Percent
    {
        get { return (int)GetValue(Panel1PercentProperty); }
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            SetValue(Panel1PercentProperty, value);
        }
    }
    public static readonly DependencyProperty Panel1PercentProperty =
        DependencyProperty.Register(nameof(Panel1Percent), typeof(int), typeof(WpfSplitGridH),
            new PropertyMetadata(50, OnPanel1PercentChanged));

    private static async void OnPanel1PercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfSplitGridH panelH)
        {
            await panelH.ToMiddleAsync();
        }
    }

    public bool FixedPanel
    {
        get { return (bool)GetValue(FixedPanelProperty); }
        set { SetValue(FixedPanelProperty, value); }
    }
    public static readonly DependencyProperty FixedPanelProperty =
        DependencyProperty.Register(nameof(FixedPanel), typeof(bool), typeof(WpfSplitGridH), new PropertyMetadata(false));

    public GridLength Panel1GridLength => new(Panel1Percent, GridUnitType.Star);
    public GridLength Panel2GridLength => new (100 - Panel1Percent, GridUnitType.Star);

    private Border? PART_RootBorder;
    private Grid? PART_MainGrid;
    private GridSplitter? PART_GridSplitter;
    private Border? PART_SplitterBorder;
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
        }

        PART_Content1Grid = GetTemplateChild(nameof(PART_Content1Grid)) as Grid;
        PART_Content2Grid = GetTemplateChild(nameof(PART_Content2Grid)) as Grid;
    }

    public WpfSplitGridH()
    {
        DataContext = this;
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            SplitterBrush = Brushes.DodgerBlue;
        }
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
                for (int n = p1Percent; n <= Panel1Percent; n++)
                {
                    if (animSpeed > 0 && n > p1Percent + 10 && n < Panel1Percent - 10) n += animSpeed;
                    PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                    PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                    if (n == Panel1Percent) break;
                    await Task.Delay(1);
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
        }
        catch (Exception) { }
    }

    private async Task MiddleToBottomAsync()
    {
        try
        {
            if (PART_MainGrid == null) return;
            int animSpeed = 0;
            if (100 - Panel1Percent >= 40) animSpeed = 2;
            if (100 - Panel1Percent >= 60) animSpeed = 3;
            for (int n = Panel1Percent; n <= 100; n++)
            {
                if (animSpeed > 0 && n > Panel1Percent + 10 && n < 90) n += animSpeed;
                PART_MainGrid.RowDefinitions[0].Height = new GridLength(n, GridUnitType.Star);
                PART_MainGrid.RowDefinitions[2].Height = new GridLength(100 - n, GridUnitType.Star);
                await Task.Delay(1);
            }
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
        }
        catch (Exception) { }
    }

    private bool IsHandling = false;

    public async void ToggleFly()
    {
        try
        {
            if (IsHandling) return;
            IsHandling = true;
            ToggleState toggleState = GetToggleStatus();
            if (toggleState == ToggleState.Unknown)
            {
                // To Middle
                await ToMiddleAsync();
            }
            else if (toggleState == ToggleState.Middle)
            {
                // Middle To Bottom
                await MiddleToBottomAsync();
            }
            else if (toggleState == ToggleState.Bottom)
            {
                // Bottom To Top
                await BottomToTopAsync();
            }
            else if (toggleState == ToggleState.Top)
            {
                // Top To Middle
                await ToMiddleAsync();
            }
            IsHandling = false;
        }
        catch (Exception) { }
    }

    private void PART_SplitterBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ToggleFly();
    }

}