using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MsmhToolsWpfClass.Themes;

namespace MsmhToolsWpfClass;

public class WpfSlidePanelHT : Control
{
    [Category("Brush")]
    public Brush SplitterBrush
    {
        get { return (Brush)GetValue(SplitterBrushProperty); }
        set { SetValue(SplitterBrushProperty, value); }
    }
    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(WpfSlidePanelHT),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrush)));

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfSlidePanelHT),
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
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(WpfSlidePanelHT), new PropertyMetadata(nameof(WpfSlidePanelHT)));

    [Category("Common")]
    public object Panel1
    {
        get { return GetValue(Panel1Property); }
        set { SetValue(Panel1Property, value); }
    }
    public static readonly DependencyProperty Panel1Property =
        DependencyProperty.Register(nameof(Panel1), typeof(object), typeof(WpfSlidePanelHT), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public object Panel2
    {
        get { return GetValue(Panel2Property); }
        set { SetValue(Panel2Property, value); }
    }
    public static readonly DependencyProperty Panel2Property =
        DependencyProperty.Register(nameof(Panel2), typeof(object), typeof(WpfSlidePanelHT), new PropertyMetadata(string.Empty));

    [Category("Common")]
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(WpfSlidePanelHT),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static async void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfSlidePanelHT panelHT)
        {
            await panelHT.UpdatePositionAsync(true);
        }
    }

    private const int AnimDurationMS = 600;

    private Border? PART_RootBorder;
    private Grid? PART_MainGrid;
    private Border? PART_SplitterBorder;
    private Ellipse? PART_Ellipse;
    private Grid? PART_Content1Grid;
    private TranslateTransform? PART_TransformContent1;
    private Grid? PART_Content2Grid;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_RootBorder = GetTemplateChild(nameof(PART_RootBorder)) as Border;
        PART_RootBorder?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_RootBorder); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

        PART_MainGrid = GetTemplateChild(nameof(PART_MainGrid)) as Grid;
        PART_MainGrid?.ClipTo(GetMaxCornerRadius, GetMaxCornerRadius, PART_MainGrid); // I Write Grid.Clip In Code Behind To Avoid Buggysoft Runtime Errors!!

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
        PART_Content1Grid = GetTemplateChild(nameof(PART_Content1Grid)) as Grid;
        PART_TransformContent1 = GetTemplateChild(nameof(PART_TransformContent1)) as TranslateTransform;
        PART_Content2Grid = GetTemplateChild(nameof(PART_Content2Grid)) as Grid;
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

    public WpfSlidePanelHT()
    {
        DataContext = this;
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            SplitterBrush = Brushes.DodgerBlue;
        }

        Loaded -= WpfSlidePanelHT_Loaded;
        Loaded += WpfSlidePanelHT_Loaded;
    }

    private async void WpfSlidePanelHT_Loaded(object sender, RoutedEventArgs e)
    {
        await UpdatePositionAsync(false);
    }

    private bool IsAnimating = false;

    private async Task UpdatePositionAsync(bool animate)
    {
        try
        {
            if (PART_SplitterBorder == null) return;
            if (PART_Content1Grid == null) return;
            if (PART_TransformContent1 == null) return;
            if (PART_Content2Grid == null) return;

            if (IsAnimating) return;
            IsAnimating = true;

            MinHeight = PART_Content1Grid.ActualHeight;

            double slideHeight = -(PART_Content1Grid.ActualHeight - PART_SplitterBorder.ActualHeight);
            double from = PART_TransformContent1.Y;
            double to = IsOpen ? 0 : slideHeight;

            if (animate)
            {
                if (IsOpen)
                {
                    PART_Content2Grid.IsHitTestVisible = false;
                    Task animSlide = PART_Content1Grid.AnimSlideVAsync(to, AnimDurationMS);
                    Task animBlur = PART_Content2Grid.AnimBlurInAsync(10, AnimDurationMS);
                    await Task.WhenAll(animSlide, animBlur);
                }
                else
                {
                    Task animSlide = PART_Content1Grid.AnimSlideVAsync(to, AnimDurationMS);
                    Task animBlur = PART_Content2Grid.AnimBlurOutAsync(AnimDurationMS);
                    await Task.WhenAll(animSlide, animBlur);
                    PART_Content2Grid.IsHitTestVisible = true;
                }
            }
            else
            {
                if (IsOpen)
                {
                    PART_Content2Grid.IsHitTestVisible = false;
                    PART_TransformContent1.Y = to;
                    await PART_Content2Grid.AnimBlurInAsync(10, 1);
                }
                else
                {
                    PART_TransformContent1.Y = to;
                    PART_Content2Grid.Effect = null;
                    PART_Content2Grid.IsHitTestVisible = true;
                }
            }

            IsAnimating = false;
        }
        catch (Exception) { }
    }

}