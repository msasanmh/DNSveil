using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfToggleSwitch.xaml
/// </summary>
public partial class WpfToggleSwitch : CheckBox
{
    public class CheckedChangedEventArgs : EventArgs
    {
        public bool? IsChecked { get; private set; }

        public CheckedChangedEventArgs(bool? isChecked)
        {
            IsChecked = isChecked;
        }
    }
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    [Category("Brush")]
    public Brush SwitchBarBrush
    {
        get { return (Brush)GetValue(SwitchBarBrushProperty); }
        set { SetValue(SwitchBarBrushProperty, value); }
    }
    public static readonly DependencyProperty SwitchBarBrushProperty =
        DependencyProperty.Register(nameof(SwitchBarBrush), typeof(Brush), typeof(WpfToggleSwitch),
            new PropertyMetadata(Brushes.LightSkyBlue));

    [Category("Brush")]
    public Brush SwitchBrush
    {
        get { return (Brush)GetValue(SwitchBrushProperty); }
        set { SetValue(SwitchBrushProperty, value); }
    }
    public static readonly DependencyProperty SwitchBrushProperty =
        DependencyProperty.Register(nameof(SwitchBrush), typeof(Brush), typeof(WpfToggleSwitch),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.DodgerBlueBrush)));

    [Category("Brush")]
    public Brush SwitchStrokeBrush
    {
        get { return (Brush)GetValue(SwitchStrokeBrushProperty); }
        set { SetValue(SwitchStrokeBrushProperty, value); }
    }
    public static readonly DependencyProperty SwitchStrokeBrushProperty =
        DependencyProperty.Register(nameof(SwitchStrokeBrush), typeof(Brush), typeof(WpfToggleSwitch),
            new PropertyMetadata(Brushes.LightSkyBlue));

    [Category("Layout")]
    public Stretch Stretch
    {
        get { return (Stretch)GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }
    public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(WpfToggleSwitch),
                new PropertyMetadata(Stretch.None));

    [Category("Common")]
    public object ContentLeft
    {
        get { return GetValue(ContentLeftProperty); }
        set { SetValue(ContentLeftProperty, value); }
    }
    public static readonly DependencyProperty ContentLeftProperty =
        DependencyProperty.Register(nameof(ContentLeft), typeof(object), typeof(WpfToggleSwitch), new PropertyMetadata(null));

    public bool HasContentLeft
    {
        get
        {
            if (ContentLeft == null) return false;
            string? cStr = ContentLeft.ToString();
            return !string.IsNullOrEmpty(cStr);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    private new Thickness Padding { get; set; }

    private Border? PART_MainBorder;
    private Border? PART_Bar;
    private Canvas? PART_Canvas;
    private Ellipse? PART_Ellipse;
    private ContentPresenter? PART_ContentLeft;
    private ContentPresenter? PART_Content;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_MainBorder = GetTemplateChild(nameof(PART_MainBorder)) as Border;
        PART_Bar = GetTemplateChild(nameof(PART_Bar)) as Border;
        PART_Canvas = GetTemplateChild(nameof(PART_Canvas)) as Canvas;
        PART_Ellipse = GetTemplateChild(nameof(PART_Ellipse)) as Ellipse;
        PART_ContentLeft = GetTemplateChild(nameof(PART_ContentLeft)) as ContentPresenter;
        PART_Content = GetTemplateChild(nameof(PART_Content)) as ContentPresenter;
    }

    public WpfToggleSwitch()
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfToggleSwitch), new FrameworkPropertyMetadata(typeof(WpfToggleSwitch)));
        DataContext = this;

        Thickness padding = new(0);
        if (Padding != padding) Padding = padding;

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            SwitchBarBrush = Brushes.MediumSeaGreen;
            SwitchBrush = Brushes.DarkGreen;
            SwitchStrokeBrush = Brushes.MediumSeaGreen;
        }

        Loaded -= WpfToggleSwitch_Loaded;
        Loaded += WpfToggleSwitch_Loaded;
        Click -= WpfToggleSwitch_Click;
        Click += WpfToggleSwitch_Click;
        Checked -= WpfToggleSwitch_Click;
        Checked += WpfToggleSwitch_Click;
        Unchecked -= WpfToggleSwitch_Click;
        Unchecked += WpfToggleSwitch_Click;
    }

    private bool IsWorking = false;
    private double PreviousX = 0;

    private async Task HandleSwitchPositionAsync()
    {
        if (PART_MainBorder == null) return;
        if (PART_Bar == null) return;
        if (PART_Canvas == null) return;
        if (PART_Ellipse == null) return;
        if (PART_ContentLeft == null) return;
        if (PART_Content == null) return;
        if (IsWorking) return;
        IsWorking = true;
        
        try
        {
            double x;
            double barOpacity, leftOpacity, rightOpacity;
            if (IsChecked.HasValue)
            {
                if (IsChecked.Value)
                {
                    // Right
                    double b = PART_MainBorder.BorderThickness.Left + PART_MainBorder.BorderThickness.Right;
                    double cm = PART_Canvas.Margin.Left + PART_Canvas.Margin.Right;
                    x = PART_MainBorder.ActualWidth - b - cm - PART_Ellipse.ActualWidth;
                    barOpacity = 0.7;
                    leftOpacity = 0.5;
                    rightOpacity = 1;
                }
                else
                {
                    // Left
                    x = 0;
                    barOpacity = 0.3;
                    leftOpacity = 1;
                    rightOpacity = 0.5;
                }
            }
            else
            {
                // Middle
                double mb = (PART_MainBorder.BorderThickness.Left + PART_MainBorder.BorderThickness.Right) / 2;
                double mcm = (PART_Canvas.Margin.Left + PART_Canvas.Margin.Right) / 2;
                x = (PART_MainBorder.ActualWidth / 2) - mb - mcm - (PART_Ellipse.ActualWidth / 2);
                barOpacity = 0.5;
                leftOpacity = 0.5;
                rightOpacity = 0.5;
            }

            if (HasContentLeft) barOpacity = 0.4;

            DoubleAnimation animX = new(PreviousX, x, new Duration(TimeSpan.FromMilliseconds(200))); // Toggle Anim
            PART_Ellipse.BeginAnimation(Canvas.LeftProperty, animX);
            DoubleAnimation animBarOpacity = new(barOpacity, new Duration(TimeSpan.FromMilliseconds(200))); // Bar Opacity Anim
            PART_Bar.BeginAnimation(OpacityProperty, animBarOpacity);
            DoubleAnimation animLeftOpacity = new(leftOpacity, new Duration(TimeSpan.FromMilliseconds(200))); // Left Content Opacity Anim
            PART_ContentLeft.BeginAnimation(OpacityProperty, animLeftOpacity);
            DoubleAnimation animRightOpacity = new(rightOpacity, new Duration(TimeSpan.FromMilliseconds(200))); // Right Content Opacity Anim
            PART_Content.BeginAnimation(OpacityProperty, animRightOpacity);
            await Task.Delay(200);
            PreviousX = x;

            // Event
            CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(IsChecked));
        }
        catch (Exception) { }

        IsWorking = false;
    }

    private async void WpfToggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        await HandleSwitchPositionAsync();
    }

    private async void WpfToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        await HandleSwitchPositionAsync();
    }
}