using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DNSveil.ManageDns;

public class IsOnlineBrushConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new IsOnlineBrushConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string str) return Brushes.Transparent;
            return str.Equals("True", StringComparison.OrdinalIgnoreCase) ? Brushes.MediumSeaGreen : Brushes.IndianRed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServers Converter IsOnlineBrushConverter: " + ex.Message);
            return Brushes.Transparent;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Brushes.Transparent;
    }
}

public class HasFilterBrushConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new HasFilterBrushConverter();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string str) return Brushes.Transparent;
            return str.Equals("True", StringComparison.OrdinalIgnoreCase) ? Brushes.IndianRed : Brushes.MediumSeaGreen;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ManageServers Converter HasFilterBrushConverter: " + ex.Message);
            return Brushes.Transparent;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Brushes.Transparent;
    }
}