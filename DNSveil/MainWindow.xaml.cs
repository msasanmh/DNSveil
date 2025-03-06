using DNSveil.DnsServers;
using DNSveil.ManageServers;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DNSveil;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly string NL = Environment.NewLine;
    private static readonly string LS = "      ";

    private AppSettings AppSettings = new(Application.Current.MainWindow);
    public static readonly string SettingsXmlPath = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Settings.xml"));

    public static readonly string DNSveilServersXmlPath = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Servers.xml"));
    public static readonly DnsServersManager ServersManager = new();

    public MainWindow()
    {
        InitializeComponent();
        
        // Set Main Page
        //FrameMain.NavigationService.Navigate(new Uri("PageMain.xaml", UriKind.Relative));
    }

    private async void RootWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AppTheme.SetTheme(Application.Current.MainWindow, Application.Current.Resources.MergedDictionaries, AppTheme.Theme.Dark);

        //// Initialize And Load Settings
        //if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
        //    AppSettings = new(Application.Current.MainWindow, SettingsXmlPath);
        //else
        //    AppSettings = new(Application.Current.MainWindow);

        // Load XML
        bool isInitialized = await ServersManager.InitializeAsync(DNSveilServersXmlPath, Application.Current.MainWindow);
        Debug.WriteLine("XDocument Initialized: " + isInitialized);
        if (!isInitialized) return;
    }

    private void RootWindow_ContentRendered(object sender, EventArgs e)
    {

    }

    private async void ButtonExit_Click(object sender, RoutedEventArgs e)
    {

        List<string> elements = ServersManager.Get_Group_Names(true);
        foreach (string element in elements)
        {
            Debug.WriteLine(element);
        }

        return;

        //string input = "My Custom Input. This Is My Custom Input Asshole! This Is My Custom Input Asshole! This Is My Custom Input Asshole!";
        //WpfInputBox.Show(ref input, "Enter DNS Address:", true);
        //Debug.WriteLine(input);

        //MessageBoxResult mbr = WpfMessageBox.Show("The Message Box");
        //Debug.WriteLine(mbr);


        AppTheme.SwitchTheme(Application.Current.MainWindow, Application.Current.Resources.MergedDictionaries);

        TestWindow testWindow = new();
        testWindow.Show();

        //List<Control> listC = Controllers.GetAllControls(this);
        //Debug.WriteLine("++++++++++++++++++++ Count: " + listC.Count);
        //foreach (Control c in listC)
        //{
        //    Debug.WriteLine(c.Name);

        //}


        //AppSettings.AddAllSettings();
        //await AppSettings.SaveAsync(SettingsXmlPath);
        Debug.WriteLine("YYYYYYYYYYYYYYYY " + AppTheme.GetColor(Application.Current.Resources.MergedDictionaries, AppTheme.DodgerBlueBrush));

    }

    private void ButtonView_Click(object sender, RoutedEventArgs e)
    {
        MainSplitPanel.ToggleFly();
    }

    private void ManageServers_Click(object sender, RoutedEventArgs e)
    {
        ManageServersWindow msw = new();
        Window? owner = this.GetParentWindow();
        if (owner != null) msw.Owner = owner;
        msw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ServersManager.UI_Window = msw;
        msw.ShowDialog();
    }

    private void IntegerUpDown_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine("HHHHHHHHHHHHHHHHHH: " + e.NewValue);
    }

}