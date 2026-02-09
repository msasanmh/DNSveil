using System.Diagnostics;
using System.Globalization;

namespace MsmhToolsClass;

public static class CultureTool
{
    public static RegionInfo GetDefaultRegion
    {
        get
        {
            RegionInfo regionInfo;
            try
            {
                regionInfo = new("en-001"); // 001 (world)
            }
            catch (Exception)
            {
                regionInfo = RegionInfo.CurrentRegion;
            }
            return regionInfo;
        }
    }

    /// <summary>
    /// Same As System RegionInfo But Serializable
    /// </summary>
    public class RegionResult
    {
        public string CurrencyEnglishName { get; set; } = string.Empty;
        public string CurrencyNativeName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public int GeoId { get; set; }
        public bool IsMetric { get; set; }
        public string ISOCurrencySymbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string ThreeLetterISORegionName { get; set; } = string.Empty;
        public string ThreeLetterWindowsRegionName { get; set; } = string.Empty;
        public string TwoLetterISORegionName { get; set; } = string.Empty;
        public bool IsFilledWithData
        {
            get
            {
                bool b1 = !string.IsNullOrEmpty(ThreeLetterISORegionName) && !string.IsNullOrEmpty(ThreeLetterWindowsRegionName) && !string.IsNullOrEmpty(TwoLetterISORegionName);
                bool b2 = !string.IsNullOrEmpty(EnglishName);
                bool b3 = !TwoLetterISORegionName.Equals("001");
                return b1 && b2 && b3;
            }
        }

        public RegionResult() { }

        public RegionResult(string currencyEnglishName, string currencyNativeName, string currencySymbol, string displayName, string englishName, int geoId, bool isMetric, string iSOCurrencySymbol, string name, string nativeName, string threeLetterISORegionName, string threeLetterWindowsRegionName, string twoLetterISORegionName)
        {
            CurrencyEnglishName = currencyEnglishName;
            CurrencyNativeName = currencyNativeName;
            CurrencySymbol = currencySymbol;
            DisplayName = displayName;
            EnglishName = englishName;
            GeoId = geoId;
            IsMetric = isMetric;
            ISOCurrencySymbol = iSOCurrencySymbol;
            Name = name;
            NativeName = nativeName;
            ThreeLetterISORegionName = threeLetterISORegionName;
            ThreeLetterWindowsRegionName = threeLetterWindowsRegionName;
            TwoLetterISORegionName = twoLetterISORegionName;
        }

        public RegionResult(RegionInfo regionInfo)
        {
            CurrencyEnglishName = regionInfo.CurrencyEnglishName;
            CurrencyNativeName = regionInfo.CurrencyNativeName;
            CurrencySymbol = regionInfo.CurrencySymbol;
            DisplayName = regionInfo.DisplayName;
            EnglishName = regionInfo.EnglishName;
            GeoId = regionInfo.GeoId;
            IsMetric = regionInfo.IsMetric;
            ISOCurrencySymbol = regionInfo.ISOCurrencySymbol;
            Name = regionInfo.Name;
            NativeName = regionInfo.NativeName;
            ThreeLetterISORegionName = regionInfo.ThreeLetterISORegionName;
            ThreeLetterWindowsRegionName = regionInfo.ThreeLetterWindowsRegionName;
            TwoLetterISORegionName = regionInfo.TwoLetterISORegionName;
        }

        public override string ToString()
        {
            string result = string.Empty;
            try
            {
                string nl = Environment.NewLine;
                result += $"{nameof(CurrencyEnglishName)}: {CurrencyEnglishName}{nl}";
                result += $"{nameof(CurrencyNativeName)}: {CurrencyNativeName}{nl}";
                result += $"{nameof(CurrencySymbol)}: {CurrencySymbol}{nl}";
                result += $"{nameof(DisplayName)}: {DisplayName}{nl}";
                result += $"{nameof(EnglishName)}: {EnglishName}{nl}";
                result += $"{nameof(GeoId)}: {GeoId}{nl}";
                result += $"{nameof(IsMetric)}: {IsMetric}{nl}";
                result += $"{nameof(ISOCurrencySymbol)}: {ISOCurrencySymbol}{nl}";
                result += $"{nameof(Name)}: {Name}{nl}";
                result += $"{nameof(NativeName)}: {NativeName}{nl}";
                result += $"{nameof(ThreeLetterISORegionName)}: {ThreeLetterISORegionName}{nl}";
                result += $"{nameof(ThreeLetterWindowsRegionName)}: {ThreeLetterWindowsRegionName}{nl}";
                result += $"{nameof(TwoLetterISORegionName)}: {TwoLetterISORegionName}";
            }
            catch (Exception) { }
            return result;
        }
    }

    public static List<CultureInfo> GetCultures(this RegionInfo regionInfo)
    {
        List<CultureInfo> cultures = new();

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue; // A Region Cannot Be Created From Neutral Culture
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue; // A Region Cannot Be Created From Invariant Culture

                RegionInfo ri = new(cultureInfo.Name); // LCID Won't Generate A Useful RegionInfo
                if (ri.TwoLetterISORegionName.Equals(regionInfo.TwoLetterISORegionName, StringComparison.OrdinalIgnoreCase))
                {
                    cultures.Add(cultureInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool Extension GetCultures: " + ex.Message);
        }

        return cultures;
    }

    public static List<CultureInfo> GetCultures_ByTwoLetter(string twoLetterISOCountryCode)
    {
        List<CultureInfo> cultures = new();

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue;
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;

                RegionInfo ri = new(cultureInfo.Name);
                if (ri.TwoLetterISORegionName.Equals(twoLetterISOCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    cultures.Add(cultureInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool GetCultures_ByTwoLetter: " + ex.Message);
        }

        return cultures;
    }

    public static List<CultureInfo> GetCultures_ByThreeLetter(string threeLetterISOCountryCode)
    {
        List<CultureInfo> cultures = new();

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue;
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;

                RegionInfo ri = new(cultureInfo.Name);
                if (ri.ThreeLetterISORegionName.Equals(threeLetterISOCountryCode, StringComparison.OrdinalIgnoreCase) ||
                    ri.ThreeLetterWindowsRegionName.Equals(threeLetterISOCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    cultures.Add(cultureInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool GetCultures_ByThreeLetter: " + ex.Message);
        }

        return cultures;
    }

    public static RegionInfo GetRegion_ByTwoLetter(string twoLetterISOCountryCode)
    {
        RegionInfo regionInfo = GetDefaultRegion;

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue;
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;

                RegionInfo ri = new(cultureInfo.Name);
                if (ri.TwoLetterISORegionName.Equals(twoLetterISOCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    regionInfo = ri;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool GetRegion_ByTwoLetter: " + ex.Message);
        }

        return regionInfo;
    }

    public static RegionInfo GetRegion_ByThreeLetter(string threeLetterISOCountryCode)
    {
        RegionInfo regionInfo = GetDefaultRegion;

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue;
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;

                RegionInfo ri = new(cultureInfo.Name);
                if (ri.ThreeLetterISORegionName.Equals(threeLetterISOCountryCode, StringComparison.OrdinalIgnoreCase) ||
                    ri.ThreeLetterWindowsRegionName.Equals(threeLetterISOCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    regionInfo = ri;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool GetRegion_ByThreeLetter: " + ex.Message);
        }

        return regionInfo;
    }

    public static List<RegionInfo> GetAllRegions()
    {
        List<RegionInfo> regionInfos = new();

        try
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (int n = 0; n < cultureInfos.Length; n++)
            {
                CultureInfo cultureInfo = cultureInfos[n];
                if (cultureInfo.IsNeutralCulture) continue; // A Region Cannot Be Created From Neutral Culture
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue; // A Region Cannot Be Created From Invariant Culture

                RegionInfo regionInfo = new(cultureInfo.Name); // LCID Won't Generate A Useful RegionInfo
                if (regionInfo.TwoLetterISORegionName.Length != 2) continue;
                regionInfos.Add(regionInfo);
            }

            // Dedup And Sort
            regionInfos = regionInfos.DistinctBy(_ => _.TwoLetterISORegionName).OrderBy(_ => _.TwoLetterISORegionName).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CultureTool GetAllRegions: " + ex.Message);
        }

        return regionInfos;
    }

}