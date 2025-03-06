using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MsmhToolsWpfClass;

/// <summary>
/// Interaction logic for NumericUpDown.xaml
/// </summary>
public partial class WpfNumericUpDown : Control
{
    [Category("Layout")]
    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WpfNumericUpDown),
            new PropertyMetadata(new CornerRadius(5)));

    [Category("Common")]
    public double Maximum
    {
        get { return (double)GetValue(MaximumProperty); }
        set { SetValue(MaximumProperty, value); }
    }
    public readonly static DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(WpfNumericUpDown),
            new UIPropertyMetadata(double.MaxValue));

    [Category("Common")]
    public double Minimum
    {
        get { return (double)GetValue(MinimumProperty); }
        set { SetValue(MinimumProperty, value); }
    }
    public readonly static DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(WpfNumericUpDown),
            new UIPropertyMetadata(double.MinValue));

    [Category("Common")]
    public double Step
    {
        get { return (double)GetValue(StepProperty); }
        set { SetValue(StepProperty, value); }
    }
    public readonly static DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(WpfNumericUpDown), new UIPropertyMetadata(1.0));

    [Category("Common")]
    public int Increment
    {
        get { return (int)GetValue(IncrementProperty); }
        set { SetValue(IncrementProperty, value); }
    }
    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(nameof(Increment), typeof(int), typeof(WpfNumericUpDown), new PropertyMetadata(1));

    // Value
    public event EventHandler<DependencyPropertyChangedEventArgs>? ValueChanged;

    private void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    [Category("Common")]
    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set
        {
            if (value > Maximum) value = Maximum;
            if (value < Minimum) value = Minimum;
            SetValue(ValueProperty, value);
        }
    }
    public readonly static DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(WpfNumericUpDown),
            new UIPropertyMetadata(0.0, (s, e) =>
            {
                if (s is WpfNumericUpDown numericUpDown)
                {
                    numericUpDown.RaiseValueChangedEvent(e);
                }
            }));

    // IsFocused
    public event EventHandler<DependencyPropertyChangedEventArgs>? IsFocusedChanged;

    private void RaiseIsFocusedChangedEvent(DependencyPropertyChangedEventArgs e)
    {
        IsFocusedChanged?.Invoke(this, e);
    }

    public new bool IsFocused
    {
        get { return (bool)GetValue(IsFocusedProperty); }
        set { SetValue(IsFocusedProperty, value); }
    }
    public static new readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.Register(nameof(IsFocused), typeof(bool), typeof(WpfNumericUpDown),
            new PropertyMetadata(false, (s, e) =>
            {
                if (s is WpfNumericUpDown numericUpDown)
                    numericUpDown.RaiseIsFocusedChangedEvent(e);
            }));

    private RepeatButton? PART_UpButton;
    private RepeatButton? PART_DownButton;
    private TextBox? PART_TextBox;

    public override void OnApplyTemplate()
    {
        PART_UpButton = GetTemplateChild(nameof(PART_UpButton)) as RepeatButton;
        if (PART_UpButton != null)
        {
            PART_UpButton.Click -= BtUp_Click;
            PART_UpButton.Click += BtUp_Click;
        }

        PART_DownButton = GetTemplateChild(nameof(PART_DownButton)) as RepeatButton;
        if (PART_DownButton != null)
        {
            PART_DownButton.Click -= BtDown_Click;
            PART_DownButton.Click += BtDown_Click;
        }

        PART_TextBox = GetTemplateChild(nameof(PART_TextBox)) as TextBox;
        if (PART_TextBox != null )
        {
            PART_TextBox.PreviewKeyDown -= PART_TextBox_PreviewKeyDown;
            PART_TextBox.PreviewKeyDown += PART_TextBox_PreviewKeyDown;
            PART_TextBox.TextChanged -= PART_TextBox_TextChanged;
            PART_TextBox.TextChanged += PART_TextBox_TextChanged;
            PART_TextBox.GotFocus -= PART_TextBox_GotFocus;
            PART_TextBox.GotFocus += PART_TextBox_GotFocus;
            PART_TextBox.LostFocus -= PART_TextBox_LostFocus;
            PART_TextBox.LostFocus += PART_TextBox_LostFocus;
        }

        base.OnApplyTemplate();
    }

    public WpfNumericUpDown()
    {
        InitializeComponent();
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfNumericUpDown), new FrameworkPropertyMetadata(typeof(WpfNumericUpDown)));
        DataContext = this;
        MinWidth = 36; MaxWidth = 100;
        IsFocused = false;
    }

    private bool ValueChangedByButton = false;

    private async void BtUp_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (Value < Maximum)
            {
                double value = Value;
                value += Step;
                value = Math.Round(value, Increment, MidpointRounding.AwayFromZero);
                if (value > Maximum) value = Maximum;
                ValueChangedByButton = true;
                Value = value;
                await Task.Delay(10);
                ValueChangedByButton = false;
            }
        }
        catch (Exception) { }
    }

    private async void BtDown_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            if (Value > Minimum)
            {
                double value = Value;
                value -= Step;
                value = Math.Round(value, Increment, MidpointRounding.ToZero);
                if (value < Minimum) value = Minimum;
                ValueChangedByButton = true;
                Value = value;
                await Task.Delay(10);
                ValueChangedByButton = false;
            }
        }
        catch (Exception) { }
    }

    private void PART_TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Up) BtUp_Click(null, null);
        else if (e.Key == System.Windows.Input.Key.Down) BtDown_Click(null, null);
    }

    private void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is TextBox textBox)
            {
                if (ValueChangedByButton)
                {
                    textBox.DispatchIt(() => textBox.CaretIndex = textBox.Text.Length);
                    return;
                }

                double value = 0;
                bool isNumber = false;
                bool isStepInt = int.TryParse(Step.ToString(), out _);
                if (isStepInt)
                {
                    bool isInt = int.TryParse(textBox.Text, out int valueOut);
                    if (isInt)
                    {
                        isNumber = true;
                        value = valueOut;
                    }
                }
                else
                {
                    if (textBox.Text.EndsWith('.')) return;
                    bool isStepDouble = double.TryParse(Step.ToString(), out _);
                    if (isStepDouble)
                    {
                        bool isDouble = double.TryParse(textBox.Text, out double valueOut);
                        if (isDouble)
                        {
                            isNumber = true;
                            value = valueOut;
                        }
                    }
                }
                
                if (isNumber)
                {
                    if (value > Maximum) value = Maximum;
                    if (value < Minimum) value = Minimum;
                    value = Math.Round(value, Increment, MidpointRounding.ToZero);
                    Value = value;
                    textBox.DispatchIt(() =>
                    {
                        textBox.Text = Value.ToString();
                        textBox.CaretIndex = textBox.Text.Length;
                    });
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.DispatchIt(() =>
                        {
                            textBox.Text = Value.ToString();
                            textBox.CaretIndex = textBox.Text.Length;
                        });
                    }
                }
            }
        }
        catch (Exception) { }
    }

    private void PART_TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not TextBox textBox) return;
            IsFocused = true;
            textBox.DispatchIt(() => textBox.CaretIndex = textBox.Text.Length);
        }
        catch (Exception) { }
    }

    private void PART_TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not TextBox textBox) return;
            IsFocused = false;
            bool isInt = int.TryParse(textBox.Text, out _);
            if (!isInt) textBox.DispatchIt(() => textBox.Text = Value.ToString());
        }
        catch (Exception) { }
    }

}