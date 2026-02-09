using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace MsmhToolsWpfClass;

public class GetPrimaryScreenWidthConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new GetPrimaryScreenWidthConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return SystemParameters.PrimaryScreenWidth;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return SystemParameters.PrimaryScreenWidth;
    }
}

public class GetPrimaryScreenHeightConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new GetPrimaryScreenHeightConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return SystemParameters.PrimaryScreenHeight;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return SystemParameters.PrimaryScreenHeight;
    }
}

// e.g. MultiBinding Converter="{local:SizeToRectMultiConverter}"
public class SizeToRectMultiConverter : MarkupExtension, IMultiValueConverter
{
    private Rect DefaultRect = new(0, 0, 300, 100);
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (values.All(x => x != null))
            {
                if (values.FirstOrDefault()?.GetType().FullName == "MS.Internal.NamedObject")
                    return DefaultRect;
                else
                {
                    double width = (double)values[0];
                    double height = (double)values[1];
                    return new Rect(0, 0, width, height);
                }
            }
            else return DefaultRect;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter SizeToRectMultiConverter: " + ex.Message);
            return DefaultRect;
        }
    }
    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return default;
    }
}

// e.g. RadiusX="{Binding CornerRadius, Converter={x:Static local:CornerRadiusToRectangleRadiusConverter.Instance}}"
public class CornerRadiusToRectangleRadiusConverter : IValueConverter // Used In WpfButton
{
    public static readonly IValueConverter Instance = new CornerRadiusToRectangleRadiusConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is CornerRadius cr)
            {
                double r1 = Math.Min(cr.TopLeft, cr.TopRight);
                double r2 = Math.Min(cr.BottomRight, cr.BottomLeft);
                double result = Math.Min(r1, r2);
                return result;
            }
            else
            {
                Debug.WriteLine("WPF Class Converter CornerRadiusToRectangleRadiusConverter: Value Is Not CornerRadius");
                return double.NaN;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter CornerRadiusToRectangleRadiusConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double d = System.Convert.ToDouble(value);
            return new CornerRadius(d);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter CornerRadiusToRectangleRadiusConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}

// CornerRadius To Thickness Converter //
// e.g. Margin="{Binding CornerRadius, Converter={x:Static local:CornerRadiusToThicknessConverter.Instance}}"
public class CornerRadiusToThicknessConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new CornerRadiusToThicknessConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CornerRadius cr)
        {
            return new Thickness(cr.TopLeft / 4, cr.TopRight / 4, cr.BottomRight / 4, cr.BottomLeft / 4);
        }
        else
        {
            Debug.WriteLine("WPF Class Converter CornerRadiusToThicknessConverter: Value Is Not CornerRadius");
            return new Thickness();
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Thickness th)
        {
            return new CornerRadius(th.Left * 4, th.Top * 4, th.Right * 4, th.Bottom * 4);
        }
        else
        {
            Debug.WriteLine("WPF Class Converter CornerRadiusToThicknessConverter ConvertBack: Value Is Not Thickness");
            return new CornerRadius();
        }
    }
}

// SelectedIndex To Visibility Converter // Used For ComboBox To Show Text Property.
public class SelectedIndexToVisibilityConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new SelectedIndexToVisibilityConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int selectedIndex = value as int? ?? -1;
        return selectedIndex == -1 ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Collapsed;
    }
}

// Reverse Bool Converter //
public class ReverseBoolConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new ReverseBoolConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        else
        {
            Debug.WriteLine("WPF Class Converter ReverseBoolConverter: Value Is Not Bool");
            return false;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        else
        {
            Debug.WriteLine("WPF Class Converter ReverseBoolConverter ConvertBack: Value Is Not Bool");
            return false;
        }
    }
}

// Bool To String Converter //
public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = "Yes";
    public string FalseValue { get; set; } = "No";
    public BoolToStringConverter() { }
    public BoolToStringConverter(string trueValue, string falseValue)
    {
        TrueValue = trueValue;
        FalseValue = falseValue;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? FalseValue : (System.Convert.ToBoolean(value) ? TrueValue : FalseValue);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && EqualityComparer<string>.Default.Equals((string)value, TrueValue);
    }
}

// String To Brush Converter //
public class StringToBrushConverter : IValueConverter
{
    public string String1 { get; set; } = "Yes";
    public string String2 { get; set; } = "No";
    public SolidColorBrush Brush1 { get; set; } = Brushes.MediumSeaGreen;
    public SolidColorBrush Brush2 { get; set; } = Brushes.IndianRed;
    public SolidColorBrush BrushOtherStrings { get; set; } = Brushes.Transparent;
    public bool ExactMatch { get; set; } = true;
    public StringToBrushConverter() { }
    public StringToBrushConverter(string string1, string string2, SolidColorBrush brush1, SolidColorBrush brush2, SolidColorBrush brushOtherStrings, bool exactMatch)
    {
        String1 = string1;
        String2 = string2;
        Brush1 = brush1;
        Brush2 = brush2;
        BrushOtherStrings = brushOtherStrings;
        ExactMatch = exactMatch;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string str) return BrushOtherStrings;
        if (ExactMatch)
            return str.Equals(String1, StringComparison.OrdinalIgnoreCase) ? Brush1 : (str.Equals(String2, StringComparison.OrdinalIgnoreCase) ? Brush2 : BrushOtherStrings);
        else
            return str.Contains(String1, StringComparison.OrdinalIgnoreCase) ? Brush1 : (str.Contains(String2, StringComparison.OrdinalIgnoreCase) ? Brush2 : BrushOtherStrings);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SolidColorBrush brush) return string.Empty;
        return brush.Equals(Brush1) ? String1 : brush.Equals(Brush2) ? String2 : string.Empty;
    }
}

// Bool To Visibility Converter //
public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new BoolToVisibilityConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            return value == null ? Visibility.Collapsed : (System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter BoolToVisibilityConverter: " + ex.Message);
            return Visibility.Visible;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && value is Visibility visibility && visibility.Equals(Visibility.Visible);
    }
}

// Math.Max Converter //
public class MathMaxConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new MathMaxConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double doubleParameter = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return Math.Max(doubleValue, doubleParameter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter MathMaxConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

// Subtract Converter //
// e.g. Width="{Binding RelativeSource={RelativeSource Mode=TemplatedParent}, Path=ActualWidth, Converter={x:Static local:SubtractConverter.Instance}, ConverterParameter=2}"
public class SubtractConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new SubtractConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double minusToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            double result = doubleValue - minusToValue;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter SubtractConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double minusToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            double result = doubleValue + minusToValue;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter SubtractConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}

public class DivideConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new DivideConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double divideByValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            if (divideByValue < 1) divideByValue = 1;
            double result = doubleValue / divideByValue;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter DivideConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double divideByValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            if (divideByValue < 1) divideByValue = 1;
            double result = doubleValue * divideByValue;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter DivideConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}

public class PercentageConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new PercentageConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture); // We Want x% Of This Value
            double percentValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            double result = percentValue * doubleValue / 100;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter PercentageConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double percentValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            double result = 100 * doubleValue / percentValue;
            if (result < 0) result = 0;
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter PercentageConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}

// IsLessThanConverter //
/*
 * e.g.
 * <DataTrigger Binding="{Binding Path=ActualWidth, RelativeSource={RelativeSource AncestorType=Window}, Converter={x:Static local:IsLessThanConverter.Instance}, ConverterParameter=1024}"
                Value="True">

                <Setter Property="ContentTemplate" Value="{StaticResource New_Layout}" />
   </DataTrigger>
 */
public class IsLessThanConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new IsLessThanConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double compareToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return doubleValue < compareToValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter IsLessThanConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double compareToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return doubleValue >= compareToValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter IsLessThanConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}

// IsGreaterThanConverter //
public class IsGreaterThanConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new IsGreaterThanConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double compareToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return doubleValue > compareToValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter IsGreaterThanConverter: " + ex.Message);
            return double.NaN;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double doubleValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double compareToValue = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return doubleValue <= compareToValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WPF Class Converter IsGreaterThanConverter ConvertBack: " + ex.Message);
            return double.NaN;
        }
    }
}
