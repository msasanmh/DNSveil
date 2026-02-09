using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MsmhToolsWpfClass.Themes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

public class WpfSlidePanelHB : Control
{
    [Category("Brush")]
    public Brush SplitterBrush
    {
        get { return (Brush)GetValue(SplitterBrushProperty); }
        set { SetValue(SplitterBrushProperty, value); }
    }
    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(WpfSlidePanelHB),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrush)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfSlidePanelHB),
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
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(WpfSlidePanelHB), new PropertyMetadata(nameof(WpfSlidePanelHB)));

    [Category("Common")]
    public object Panel1
    {
        get { return GetValue(Panel1Property); }
        set { SetValue(Panel1Property, value); }
    }
    public static readonly DependencyProperty Panel1Property =
        DependencyProperty.Register(nameof(Panel1), typeof(object), typeof(WpfSlidePanelHB), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public object Panel2
    {
        get { return GetValue(Panel2Property); }
        set { SetValue(Panel2Property, value); }
    }
    public static readonly DependencyProperty Panel2Property =
        DependencyProperty.Register(nameof(Panel2), typeof(object), typeof(WpfSlidePanelHB), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(WpfSlidePanelHB),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static async void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfSlidePanelHB panelHB)
        {
            await panelHB.UpdatePositionAsync(true);
        }
    }

    private const int AnimDurationMS = 600;

    private Border? PART_RootBorder;
    private Grid? PART_MainGrid;
    private Grid? PART_Content1Grid;
    private Grid? PART_Content2Grid;
    private Border? PART_SplitterBorder;
    private Ellipse? PART_Ellipse;
    private TranslateTransform? PART_TransformContent2;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_RootBorder = GetTemplateChild(nameof(PART_RootBorder)) as Border;
        PART_RootBorder?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_RootBorder); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

        PART_MainGrid = GetTemplateChild(nameof(PART_MainGrid)) as Grid;
        PART_MainGrid?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_MainGrid); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

        PART_Content1Grid = GetTemplateChild(nameof(PART_Content1Grid)) as Grid;
        PART_Content2Grid = GetTemplateChild(nameof(PART_Content2Grid)) as Grid;

        PART_SplitterBorder = GetTemplateChild(nameof(PART_SplitterBorder)) as Border;
        if (PART_SplitterBorder != null)
        {
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
        PART_TransformContent2 = GetTemplateChild(nameof(PART_TransformContent2)) as TranslateTransform;
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

    public WpfSlidePanelHB()
    {
        DataContext = this;
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            SplitterBrush = Brushes.DodgerBlue;
        }

        Loaded -= WpfSlidePanelHB_Loaded;
        Loaded += WpfSlidePanelHB_Loaded;
    }

    private async void WpfSlidePanelHB_Loaded(object sender, RoutedEventArgs e)
    {
        await UpdatePositionAsync(false);
    }

    private bool IsAnimating = false;

    private async Task UpdatePositionAsync(bool animate)
    {
        try
        {
            if (PART_Content1Grid == null) return;
            if (PART_Content2Grid == null) return;
            if (PART_SplitterBorder == null) return;
            if (PART_TransformContent2 == null) return;
            
            if (IsAnimating) return;
            IsAnimating = true;

            MinHeight = PART_Content2Grid.ActualHeight;

            double slideHeight = PART_Content2Grid.ActualHeight - PART_SplitterBorder.ActualHeight;
            double from = PART_TransformContent2.Y;
            double to = IsOpen ? 0 : slideHeight;

            if (animate)
            {
                if (IsOpen)
                {
                    PART_Content1Grid.IsHitTestVisible = false;
                    Task animSlide = PART_Content2Grid.AnimSlideVAsync(to, AnimDurationMS);
                    Task animBlur = PART_Content1Grid.AnimBlurInAsync(10, AnimDurationMS);
                    await Task.WhenAll(animSlide, animBlur);
                }
                else
                {
                    Task animSlide = PART_Content2Grid.AnimSlideVAsync(to, AnimDurationMS);
                    Task animBlur = PART_Content1Grid.AnimBlurOutAsync(AnimDurationMS);
                    await Task.WhenAll(animSlide, animBlur);
                    PART_Content1Grid.IsHitTestVisible = true;
                }
            }
            else
            {
                if (IsOpen)
                {
                    PART_Content1Grid.IsHitTestVisible = false;
                    PART_TransformContent2.Y = to;
                    await PART_Content1Grid.AnimBlurInAsync(10, 1);
                }
                else
                {
                    PART_TransformContent2.Y = to;
                    PART_Content1Grid.Effect = null;
                    PART_Content1Grid.IsHitTestVisible = true;
                }
            }

            IsAnimating = false;
        }
        catch (Exception) { }
    }
}