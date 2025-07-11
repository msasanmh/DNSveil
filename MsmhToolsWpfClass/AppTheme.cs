using System.Windows.Interop;
using System.Windows;
using MsmhToolsClass;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using System.Runtime.ConstrainedExecution;

namespace MsmhToolsWpfClass.Themes;

public static class AppTheme
{
    // DodgerBlue: HEX value: #005A9C RGB value: 0,90,156
    // IndianRed: HEX value: #CD5C5C RGB value: 205,92,92
    //=======================================================================================
    public enum Theme
    {
        Light,
        Dark,
        Unknown
    }
    //=======================================================================================
    // Usage:
    // xmlns:resources="clr-namespace:MsmhToolsWpfClass.Themes;assembly=MsmhToolsWpfClass"
    // Background="{DynamicResource {x:Static resources:Theme.Background}}"
    public static readonly Uri DarkThemeResourcesFile = new("pack://application:,,,/MsmhToolsWpfClass;component/AppThemeDark.xaml");
    public static readonly Uri LightThemeResourcesFile = new("pack://application:,,,/MsmhToolsWpfClass;component/AppThemeLight.xaml");


    public static ComponentResourceKey BackgroundColor { get; set; } = new(typeof(AppTheme), nameof(BackgroundColor));
    public static ComponentResourceKey BackgroundBrush { get; set; } = new(typeof(AppTheme), nameof(BackgroundBrush));
    public static ComponentResourceKey BackgroundDarkerColor { get; set; } = new(typeof(AppTheme), nameof(BackgroundDarkerColor));
    public static ComponentResourceKey BackgroundDarkerBrush { get; set; } = new(typeof(AppTheme), nameof(BackgroundDarkerBrush));

    public static ComponentResourceKey BorderColor { get; set; } = new(typeof(AppTheme), nameof(BorderColor));
    public static ComponentResourceKey BorderBrush { get; set; } = new(typeof(AppTheme), nameof(BorderBrush));

    public static ComponentResourceKey ForegroundColor { get; set; } = new(typeof(AppTheme), nameof(ForegroundColor));
    public static ComponentResourceKey ForegroundBrush { get; set; } = new(typeof(AppTheme), nameof(ForegroundBrush));
    public static ComponentResourceKey ForegroundDarkerColor { get; set; } = new(typeof(AppTheme), nameof(ForegroundDarkerColor));
    public static ComponentResourceKey ForegroundDarkerBrush { get; set; } = new(typeof(AppTheme), nameof(ForegroundDarkerBrush));

    public static ComponentResourceKey SelectedItemColor { get; set; } = new(typeof(AppTheme), nameof(SelectedItemColor));
    public static ComponentResourceKey SelectedItemBrush { get; set; } = new(typeof(AppTheme), nameof(SelectedItemBrush));

    public static ComponentResourceKey DodgerBlueColor { get; set; } = new(typeof(AppTheme), nameof(DodgerBlueColor));
    public static ComponentResourceKey DodgerBlueColorTransparent { get; set; } = new(typeof(AppTheme), nameof(DodgerBlueColorTransparent));
    public static ComponentResourceKey DodgerBlueBrush { get; set; } = new(typeof(AppTheme), nameof(DodgerBlueBrush));
    public static ComponentResourceKey DodgerBlueBrushTransparent { get; set; } = new(typeof(AppTheme), nameof(DodgerBlueBrushTransparent));
    public static ComponentResourceKey IndianRedColor { get; set; } = new(typeof(AppTheme), nameof(IndianRedColor));
    public static ComponentResourceKey IndianRedColorTransparent { get; set; } = new(typeof(AppTheme), nameof(IndianRedColorTransparent));
    public static ComponentResourceKey IndianRedBrush { get; set; } = new(typeof(AppTheme), nameof(IndianRedBrush));
    public static ComponentResourceKey IndianRedBrushTransparent { get; set; } = new(typeof(AppTheme), nameof(IndianRedBrushTransparent));
    public static ComponentResourceKey MediumSeaGreenColor { get; set; } = new(typeof(AppTheme), nameof(MediumSeaGreenColor));
    public static ComponentResourceKey MediumSeaGreenColorTransparent { get; set; } = new(typeof(AppTheme), nameof(MediumSeaGreenColorTransparent));
    public static ComponentResourceKey MediumSeaGreenBrush { get; set; } = new(typeof(AppTheme), nameof(MediumSeaGreenBrush));
    public static ComponentResourceKey MediumSeaGreenBrushTransparent { get; set; } = new(typeof(AppTheme), nameof(MediumSeaGreenBrushTransparent));

    public static ComponentResourceKey BorderThickness { get; set; } = new(typeof(AppTheme), nameof(BorderThickness));
    //=======================================================================================
    /// <summary>
    /// SetDarkTitleBar
    /// </summary>
    /// <param name="window">Window</param>
    /// <param name="darkMode">Dark: True, Light: False</param>
    /// <param name="colorInHex">Color In Hex (Only Standard WPF Colors & Windows 11 Above) e.g. 202020</param>
    public static void SetDarkTitleBar(Window window, bool darkMode, string colorInHex = "202020")
    {
        IntPtr hWnd = new WindowInteropHelper(window).EnsureHandle();
        UseImmersiveDarkMode(hWnd, darkMode, colorInHex);
    }
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWWMA_CAPTION_COLOR = 35;
    private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled, string colorInHex)
    {
        colorInHex = colorInHex.Trim();
        if (colorInHex.StartsWith('#')) colorInHex = colorInHex.TrimStart('#');
        if (OsTool.IsWindows11OrGreater() && !string.IsNullOrEmpty(colorInHex))
        {
            if (!enabled) colorInHex = "ffffff";
            colorInHex = $"0x{colorInHex}";
            int[] colorIntArray = new int[] { 0x202020 }; // Default
            try { colorIntArray = new int[] { Convert.ToInt32(colorInHex, 16) }; } catch (Exception) { }
            return NativeMethods.DwmSetWindowAttribute(handle, DWWMA_CAPTION_COLOR, colorIntArray, 4) == 0;
        }
        else if (OsTool.IsWindows10OrGreater(17763))
        {
            int attribute = OsTool.IsWindows10OrGreater(18985) ? DWMWA_USE_IMMERSIVE_DARK_MODE : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            int useImmersiveDarkMode = enabled ? 1 : 0;
            return NativeMethods.DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }
        return false;
    }
    //=======================================================================================
    public static Theme GetTheme(Collection<ResourceDictionary> applicationMergedDictionaries)
    {
        Theme themes = Theme.Unknown;

        try
        {
            List<Tuple<int, Theme>> list = new();
            for (int n = 0; n < applicationMergedDictionaries.Count; n++)
            {
                ResourceDictionary rd = applicationMergedDictionaries[n];
                if (rd.Source == DarkThemeResourcesFile) list.Add(new Tuple<int, Theme>(n, Theme.Dark));
                if (rd.Source == LightThemeResourcesFile) list.Add(new Tuple<int, Theme>(n, Theme.Light));
            }

            list = list.OrderByDescending(x => x.Item1).ToList();

            if (list.Count > 0)
            {
                themes = list[0].Item2;
                list.RemoveAt(0);
                if (list.Count > 0)
                {
                    for (int n = 0; n < list.Count; n++)
                    {
                        applicationMergedDictionaries.RemoveAt(list[n].Item1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppTheme GetTheme: " + ex.Message);
        }

        return themes;
    }
    //=======================================================================================
    public static void SetTheme(Window mainWindow, Collection<ResourceDictionary> applicationMergedDictionaries, Theme theme)
    {
        try
        {
            if (theme == Theme.Unknown) theme = Theme.Dark;
            Theme currentTheme = GetTheme(applicationMergedDictionaries);

            if (currentTheme == Theme.Unknown || currentTheme != theme)
            {
                // Remove Current Theme
                for (int n = 0; n < applicationMergedDictionaries.Count; n++)
                {
                    ResourceDictionary rd = applicationMergedDictionaries[n];
                    if (rd.Source == DarkThemeResourcesFile || rd.Source == LightThemeResourcesFile)
                    {
                        applicationMergedDictionaries.RemoveAt(n);
                        break;
                    }
                }

                // Set Theme
                ResourceDictionary resourceDictionary = new();
                if (theme == Theme.Dark)
                {
                    // Dark Theme
                    SetDarkTitleBar(mainWindow, true);
                    resourceDictionary = new() { Source = DarkThemeResourcesFile };
                    applicationMergedDictionaries.Add(resourceDictionary);
                }
                else
                {
                    // Light Theme
                    SetDarkTitleBar(mainWindow, false);
                    resourceDictionary = new() { Source = LightThemeResourcesFile };
                    applicationMergedDictionaries.Add(resourceDictionary);
                }

                Color dodgerBlue = Colors.Transparent;
                string valueStr = ResourceTool.GetValueByKey(resourceDictionary, DodgerBlueBrush);
                bool isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out Color color);
                if (isSuccess) dodgerBlue = color;

                Color mediumSeaGreen = Colors.Transparent;
                valueStr = ResourceTool.GetValueByKey(resourceDictionary, MediumSeaGreenBrush);
                isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out color);
                if (isSuccess) mediumSeaGreen = color;

                return;

                SolidColorBrush transparentBrush = new(Colors.Transparent);
                List<Control> controls = Controllers.GetAllControls(mainWindow);
                //Debug.WriteLine("controls " + controls.Count);
                for (int n = 0; n < controls.Count; n++)
                {
                    Control control = controls[n];
                    
                    if (control is WpfSlidePanelHB wpfSlidePanelHB)
                    {
                        Color clr = theme == Theme.Light ? mediumSeaGreen : dodgerBlue;
                        if (clr != Colors.Transparent)
                        {
                            wpfSlidePanelHB.SplitterBrush = new SolidColorBrush(clr);
                        }
                    }

                    if (control is WpfSlidePanelHT wpfSlidePanelHT)
                    {
                        Color clr = theme == Theme.Light ? mediumSeaGreen : dodgerBlue;
                        if (clr != Colors.Transparent)
                        {
                            wpfSlidePanelHT.SplitterBrush = new SolidColorBrush(clr);
                            //Debug.WriteLine(control.Name + " MouseOver BG=> " + color);
                        }
                    }

                    if (control is WpfSlidePanelVL wpfSlidePanelVL)
                    {
                        
                        Color clr = theme == Theme.Light ? mediumSeaGreen : dodgerBlue;
                        if (clr != Colors.Transparent)
                        {
                            wpfSlidePanelVL.SplitterBrush = new SolidColorBrush(clr);
                            //Debug.WriteLine(control.Name + " MouseOver BG=> " + color);
                        }
                    }

                    if (control is WpfSlidePanelVR wpfSlidePanelVR)
                    {
                        Color clr = theme == Theme.Light ? mediumSeaGreen : dodgerBlue;
                        if (clr != Colors.Transparent)
                        {
                            wpfSlidePanelVR.SplitterBrush = new SolidColorBrush(clr);
                            //Debug.WriteLine(control.Name + " MouseOver BG=> " + color);
                        }
                    }

                    continue;

                    if (control.Background != null && !control.Background.Equals(transparentBrush))
                    {
                        valueStr = ResourceTool.GetValueByKey(resourceDictionary, BackgroundBrush);
                        isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out color);
                        if (isSuccess)
                        {
                            control.Background = new SolidColorBrush(color);
                            //Debug.WriteLine(control.Name + " BG=> " + color);
                        }
                    }
                    
                    if (control.Foreground != null && !control.Foreground.Equals(transparentBrush))
                    {
                        valueStr = ResourceTool.GetValueByKey(resourceDictionary, ForegroundBrush);
                        isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out color);
                        if (isSuccess)
                        {
                            control.Foreground = new SolidColorBrush(color);
                            //Debug.WriteLine(control.Name + " FG=> " + color);
                        }
                    }
                    
                    if (control.BorderBrush != null && !control.BorderBrush.Equals(transparentBrush))
                    {
                        valueStr = ResourceTool.GetValueByKey(resourceDictionary, BorderBrush);
                        isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out color);
                        if (isSuccess)
                        {
                            control.BorderBrush = new SolidColorBrush(color);
                            //Debug.WriteLine(control.Name + " Border=> " + color);
                        }
                    }
                }
                return;
                List<Grid> grids = Controllers.GetAllElementsByType<Grid>(mainWindow);
                Debug.WriteLine("grids " + grids.Count);
                for (int n = 0; n < grids.Count; n++)
                {
                    Grid grid = grids[n];
                    valueStr = ResourceTool.GetValueByKey(resourceDictionary, BackgroundBrush);
                    isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out color);
                    if (isSuccess)
                    {
                        grid.Background = new SolidColorBrush(color);
                        Debug.WriteLine(grid.Name + " BG=> " + color);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppTheme SetTheme: " + ex.Message);
        }
    }
    //=======================================================================================
    // Usage: Theme.SwitchTheme(Application.Current.MainWindow, Application.Current.Resources.MergedDictionaries);
    public static Theme SwitchTheme(Window mainWindow, Collection<ResourceDictionary> applicationMergedDictionaries)
    {
        try
        {
            Theme currentTheme = GetTheme(applicationMergedDictionaries);
            Theme theme = currentTheme switch
            {
                Theme.Dark => Theme.Light,
                Theme.Light => Theme.Dark,
                _ => Theme.Dark,
            };

            SetTheme(mainWindow, applicationMergedDictionaries, theme);
            Debug.WriteLine($"Theme: {currentTheme} => {theme}");
            return theme;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppTheme SwitchTheme: " + ex.Message);
            return Theme.Unknown;
        }
    }
    //=======================================================================================
    public static Color GetColor(Collection<ResourceDictionary> applicationMergedDictionaries, ComponentResourceKey componentResourceKey)
    {
        Color result = Colors.Transparent;
        
        try
        {
            for (int n = 0; n < applicationMergedDictionaries.Count; n++)
            {
                ResourceDictionary rd = applicationMergedDictionaries[n];
                if (rd.Source == DarkThemeResourcesFile || rd.Source == LightThemeResourcesFile)
                {
                    string valueStr = ResourceTool.GetValueByKey(rd, componentResourceKey);
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        bool isSuccess = ConvertToolWpf.TryConvertColorFromString(valueStr, out Color color);
                        if (isSuccess)
                        {
                            result = color;
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AppTheme GetColor: " + ex.Message);
        }
        
        return result;
    }

    public static Color GetColor(ComponentResourceKey componentResourceKey)
    {
        return GetColor(Application.Current.Resources.MergedDictionaries, componentResourceKey);
    }

    public static Brush GetBrush(Collection<ResourceDictionary> applicationMergedDictionaries, ComponentResourceKey componentResourceKey)
    {
        return new SolidColorBrush(GetColor(applicationMergedDictionaries, componentResourceKey));
    }

    public static Brush GetBrush(ComponentResourceKey componentResourceKey)
    {
        return new SolidColorBrush(GetColor(componentResourceKey));
    }
    //=======================================================================================
}