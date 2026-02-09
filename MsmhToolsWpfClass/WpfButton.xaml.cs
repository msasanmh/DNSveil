using MsmhToolsWpfClass.Themes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for WpfButton.xaml
/// </summary>
public partial class WpfButton : Button
{
    [Category("Brush")]
    public Brush MouseOverBrush
    {
        get { return (Brush)GetValue(MouseOverBrushProperty); }
        set { SetValue(MouseOverBrushProperty, value); }
    }
    public static readonly DependencyProperty MouseOverBrushProperty =
        DependencyProperty.Register(nameof(MouseOverBrush), typeof(Brush), typeof(WpfButton),
            new PropertyMetadata(AppTheme.GetBrush(AppTheme.MediumSeaGreenBrushTransparent)));

    [Category("Brush")]
    public Brush RippleBrush
    {
        get { return (Brush)GetValue(RippleBrushProperty); }
        set { SetValue(RippleBrushProperty, value); }
    }
    public static readonly DependencyProperty RippleBrushProperty =
        DependencyProperty.Register(nameof(RippleBrush), typeof(Brush), typeof(WpfButton),
            new PropertyMetadata(Brushes.White));

    [Category("Layout")]
    public Stretch Stretch
    {
        get { return (Stretch)GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }
    public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(WpfButton),
                new PropertyMetadata(Stretch.None));

    private CornerRadius BackupCornerRadius { get; set; } = new CornerRadius(5);

    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfButton),
                new PropertyMetadata(new CornerRadius(5), (s, e) =>
                {
                    if (s is WpfButton wpfButton)
                    {
                        try
                        {
                            if (!wpfButton.RoundButton) wpfButton.BackupCornerRadius = wpfButton.CornerRadius;
                        }
                        catch (Exception) { }
                    }
                }));

    [Category("Common")]
    public bool RoundButton
    {
        get { return (bool)GetValue(RoundButtonProperty); }
        set { SetValue(RoundButtonProperty, value); }
    }
    public static readonly DependencyProperty RoundButtonProperty =
        DependencyProperty.Register(nameof(RoundButton), typeof(bool), typeof(WpfButton),
            new PropertyMetadata(false, (s, e) =>
            {
                if (s is WpfButton wpfButton)
                {
                    wpfButton.SetRoundButton();
                }
            }));

    public double GetAnimScale => Math.Max(ActualWidth, ActualHeight) / 3;

    private Grid? PART_Grid;
    private TextBlock? PART_HiddenTextBlock;
    private Storyboard? PART_Storyboard;

    public override async void OnApplyTemplate()
    {
        try
        {
            base.OnApplyTemplate();

            PART_Grid = GetTemplateChild(nameof(PART_Grid)) as Grid;
            if (PART_Grid != null)
            {
                PART_Grid.PreviewMouseLeftButtonDown -= PART_Grid_PreviewMouseLeftButtonDown;
                PART_Grid.PreviewMouseLeftButtonDown += PART_Grid_PreviewMouseLeftButtonDown;
            }

            PART_HiddenTextBlock = GetTemplateChild(nameof(PART_HiddenTextBlock)) as TextBlock;
            if (PART_HiddenTextBlock != null)
            {
                if (MinWidth < PART_HiddenTextBlock.ActualWidth) MinWidth = PART_HiddenTextBlock.ActualWidth;
                if (MinHeight < PART_HiddenTextBlock.ActualHeight) MinHeight = PART_HiddenTextBlock.ActualHeight;
            }

            await Task.Delay(100);
            PART_Storyboard = (Storyboard)Resources["RippleEffect"];
            if (PART_Storyboard != null)
            {
                PART_Storyboard.Completed -= PART_Storyboard_Completed;
                PART_Storyboard.Completed += PART_Storyboard_Completed;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WpfButton OnApplyTemplate: " + ex.Message);
        }
    }

    public WpfButton()
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfButton), new FrameworkPropertyMetadata(typeof(WpfButton)));
        DataContext = this;
        Loaded -= WpfButton_Loaded;
        Loaded += WpfButton_Loaded;
    }

    private void WpfButton_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!RoundButton) BackupCornerRadius = CornerRadius;
            SetRoundButton();
        }
        catch (Exception) { }
    }

    private void SetRoundButton()
    {
        try
        {
            if (RoundButton)
            {
                if (Stretch == Stretch.None)
                {
                    if (ActualWidth >= ActualHeight)
                    {
                        Binding widthBinding = new()
                        {
                            Path = new PropertyPath(ActualWidthProperty)
                        };
                        BindingOperations.SetBinding(this, HeightProperty, widthBinding);
                        CornerRadius = new CornerRadius(ActualWidth / 2);
                    }
                    else
                    {
                        Binding heightBinding = new()
                        {
                            Path = new PropertyPath(ActualHeightProperty)
                        };
                        BindingOperations.SetBinding(this, WidthProperty, heightBinding);
                        CornerRadius = new CornerRadius(ActualHeight / 2);
                    }

                    Padding = new Thickness(4);
                }
            }
            else
            {
                BindingOperations.ClearBinding(this, WidthProperty);
                BindingOperations.ClearBinding(this, HeightProperty);
                Padding = new Thickness(8, 4, 8, 4);
                CornerRadius = BackupCornerRadius;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WpfButton SetRoundButton: " + ex.Message);
        }
    }

    private async void PART_Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PART_Storyboard == null) return;
            if (sender is Grid grid)
            {
                // Remove The Previous Storyboard
                PART_Storyboard.Remove();

                // Get Current Mouse Position
                Point mousePosition = e.GetPosition(grid);

                // Get The Canvas From The Grid
                if (grid.Children[1] is Canvas canvas)
                {
                    // Get The Ellipse From The Canvas
                    if (canvas.Children[0] is Ellipse ellipse)
                    {
                        // Move The Ellipse To Mouse Position
                        ellipse.SetValue(Canvas.LeftProperty, mousePosition.X - ellipse.ActualWidth / 2);
                        ellipse.SetValue(Canvas.TopProperty, mousePosition.Y - ellipse.ActualHeight / 2);

                        await Task.Delay(10);

                        // Attach Storyboard To The Ellipse
                        Storyboard.SetTarget(PART_Storyboard, ellipse);
                        PART_Storyboard.Begin();
                        await Task.Delay(10);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WpfButton PART_Grid_PreviewMouseLeftButtonDown: " + ex.Message);
        }
    }

    private void PART_Storyboard_Completed(object? sender, EventArgs e)
    {
        try
        {
            PART_Storyboard?.Remove();
        }
        catch (Exception) { }
    }

}