using System.Diagnostics;

#nullable enable
namespace MsmhToolsClass;

public class ConvertTool
{
    public static string TimeSpanToHumanRead(TimeSpan eTime, bool fullRead)
    {
        string days = eTime.Days.ToString().TrimStart('0').PadLeft(1, '0');
        eTime = TimeSpan.FromMilliseconds(Math.Round(eTime.TotalMilliseconds, 2));
        string hours = eTime.Hours.ToString().TrimStart('0').PadLeft(1, '0');
        string minutes = eTime.Minutes.ToString().TrimStart('0').PadLeft(1, '0');
        string seconds = eTime.Seconds.ToString().TrimStart('0').PadLeft(1, '0');
        string milliseconds = eTime.Milliseconds.ToString().TrimStart('0').PadLeft(1, '0');

        string result = string.Empty;
        if (fullRead)
        {
            if (eTime.Days > 0) result += eTime.Days > 1 ? $"{days} Days, " : $"{days} Day, ";
            if (eTime.Hours > 0) result += eTime.Hours > 1 ? $"{hours} Hours, " : $"{hours} Hour, ";
            if (eTime.Minutes > 0) result += eTime.Minutes > 1 ? $"{minutes} Minutes, " : $"{minutes} Minute, ";
            if (eTime.Seconds > 0) result += eTime.Seconds > 1 ? $"{seconds} Seconds, " : $"{seconds} Second, ";
            if (eTime.Milliseconds > 0) result += eTime.Milliseconds > 1 ? $"{milliseconds} Milliseconds" : $"{milliseconds} Millisecond";
            result = result.Trim();
            if (result.EndsWith(',')) result = result.TrimEnd(',');
        }
        else
        {
            if (eTime.Days > 0) result += $"{days}:";
            if (eTime.Hours > 0) result += $"{hours}:";
            if (eTime.Minutes > 0) result += $"{minutes}:";
            if (eTime.Seconds > 0) result += $"{seconds}.";
            if (eTime.Milliseconds > 0) result += $"{milliseconds}";
            if (result.EndsWith(':')) result = result.TrimEnd(':');
            if (result.EndsWith('.')) result = result.TrimEnd('.');
        }
        
        return result;
    }

    public enum SizeUnits
    {
        Byte, KB, MB, GB, TB, PB, EB, ZB, YB, RB, QB
    }

    public static string ConvertByteToHumanRead(double bytes)
    {
        try { bytes = Math.Round(bytes, 2); } catch (Exception) { }
        string result = $"{bytes} {SizeUnits.Byte}";
        double calc = bytes;
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.Byte, SizeUnits.KB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.KB, SizeUnits.MB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.MB, SizeUnits.GB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.GB, SizeUnits.TB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.TB, SizeUnits.PB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.PB, SizeUnits.EB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.EB, SizeUnits.ZB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.ZB, SizeUnits.YB, out result);
        if (calc > 1000) calc = ConvertSize(calc, SizeUnits.YB, SizeUnits.RB, out result);
        if (calc > 1000) ConvertSize(calc, SizeUnits.RB, SizeUnits.QB, out result);
        return result;
    }

    public static double ConvertSize(double value, SizeUnits fromUnit, SizeUnits toUnit, out string humanRead)
    {
        try
        {
            double unit = 1000; // In decimal it's 1000 not 1024. ref:https://en.wikipedia.org/wiki/Byte#Multiple-byte_units
            double valueByte = value * (double)Math.Pow(unit, (long)fromUnit);
            double outValue = valueByte / (double)Math.Pow(unit, (long)toUnit);
            string outString = outValue.ToString();
            if (outString.Contains('.') && outString.EndsWith('0'))
            {
                while (outString.EndsWith('0'))
                {
                    outString = outString.TrimEnd('0');
                }
                outString = outString.TrimEnd('.');
                double result = double.TryParse(outString, out double outValueNoZero) ? outValueNoZero : outValue;
                humanRead = $"{Math.Round(result, 2)} {toUnit}";
                return result;
            }
            else
            {
                humanRead = $"{Math.Round(outValue, 2)} {toUnit}";
                return outValue;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ConvertSize: {ex.Message}");
            humanRead = $"-1 {toUnit}";
            return -1;
        }
    }
    //-----------------------------------------------------------------------------------
}