using DNSveil.Logic;
using DNSveil.Logic.DnsServers;
using DNSveil.ManageServers;
using MsmhToolsClass;
using MsmhToolsClass.V2RayConfigTool;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace DNSveil;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly string NL = Environment.NewLine;
    private static readonly string LS = "      ";

    private AppSettings AppSettings = new(Application.Current.MainWindow);

    public static readonly DnsServersManager ServersManager = new();
    private SetDnsOnNic SetDnsOnNic_ = new();
    private Interface Interface_ = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        // Set Main Page
        //FrameMain.NavigationService.Navigate(new Uri("PageMain.xaml", UriKind.Relative));

        // Create UserData Structure
        FileDirectory.CreateEmptyDirectory(Pathes.BinaryDirPath);
        FileDirectory.CreateEmptyDirectory(Pathes.UserDataDir);
        FileDirectory.CreateEmptyDirectory(Pathes.AssetDir);
        FileDirectory.CreateEmptyDirectory(Pathes.CertificateDir);

    }

    private void RootWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Set Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);

        // Set Help: Fragment
        string f1 = "Split The Part Before The SNI/Domain Into N Pieces.";
        Help_Fragment_ChunksBeforeSNI.Content = f1;
        string f2 = $"\u2022 SNI: Capture Only Domain Name. (Recommended){NL}\u2022 SNI Extension: Capture SNI Extension.{NL}\u2022 All Extensions: Capture All Extensions In The Client Hello.";
        Help_Fragment_SniChunkMode.Content = f2;
        string f3 = "Split The SNI/Domain Into N Pieces.";
        Help_Fragment_ChunksSNI.Content = f3;
        string f4 = "Randomly Add Or Remove N Pieces To Avoid Being Recognized As A Pattern.";
        Help_Fragment_AntiPatternOffset.Content = f4;
        string f5 = "The Delay Between Sending The Pieces In Milliseconds To Avoid Detection.";
        Help_Fragment_FragmentDelayMS.Content = f5;

        // Set Help: SSL Decryption
        string help_SslDecryption = "\u2022 By Installing Self-Signed Root Certificate Authority.\n";
        help_SslDecryption += "\u2022 If Your Browser Is Already Open You Need To Restart It After Activating This Option.\n";
        help_SslDecryption += "\u2022 Enable This Option Only If Fragment Does Not Work For You.";
        Help_SslDecryption.Content = help_SslDecryption;

        // Set Theme
        AppTheme.SetTheme(Application.Current.MainWindow, Application.Current.Resources.MergedDictionaries, AppTheme.Theme.Dark);

    }

    private async void RootWindow_ContentRendered(object sender, EventArgs e)
    {
        try
        {
            // Load XML
            bool isInitialized = await ServersManager.InitializeAsync(Pathes.DnsServers_BuiltIn, Pathes.DnsServers_User, Application.Current.MainWindow);
            Debug.WriteLine("XDocument Initialized: " + isInitialized);
            if (!isInitialized)
            {
                string msg = "Failed To Initialize DNS Servers XML File.";
                WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            
            // Bind DnsGroup ComboBox
            QC_DnsGroup_ComboBox.ItemsSource = ServersManager.Get_GroupItems(true);
            QC_DnsGroup_ComboBox.DisplayMemberPath = "Name";
            QC_DnsGroup_ComboBox.SelectedValuePath = "Name";
            QC_DnsGroup_ComboBox.SelectedIndex = 0;
            QC_DnsGroup_ComboBox.MinWidth = QC_DnsGroup_ComboBox.ActualWidth * 1.5;
            QC_DnsGroup_ComboBox.MaxWidth = QC_DnsGroup_ComboBox.ActualWidth * 1.5;

            // Bind NIC ComboBox
            var updateNIC = await SetDnsOnNic_.UpdateNICs(IPAddress.Parse("8.8.8.8"), 53, selectAuto: true);
            QC_NIC_ComboBox.ItemsSource = updateNIC.AllNICs;
            QC_NIC_ComboBox.SelectedItem = updateNIC.PrimaryNIC;
            QC_NIC_ComboBox.MinWidth = QC_NIC_ComboBox.ActualWidth;
            QC_NIC_ComboBox.MaxWidth = QC_NIC_ComboBox.ActualWidth;

            // Bind Upstream ComboBox
            QC_Upstream_ComboBox.MinWidth = QC_Upstream_ComboBox.ActualWidth * 1.28;
            QC_Upstream_ComboBox.MaxWidth = QC_Upstream_ComboBox.ActualWidth * 1.28;

            // Bind Interface ComboBox
            QC_Interface_ComboBox.ItemsSource = Interface_.BindDataSource;
            QC_Interface_ComboBox.SelectedIndex = 1; // System Proxy
            QC_Interface_ComboBox.MinWidth = QC_Interface_ComboBox.ActualWidth;
            QC_Interface_ComboBox.MaxWidth = QC_Interface_ComboBox.ActualWidth;

            //// Initialize And Load Settings
            //if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
            //    AppSettings = new(Application.Current.MainWindow, SettingsXmlPath);
            //else
            //    AppSettings = new(Application.Current.MainWindow);

            // Set Min Width
            double minWidth1 = (FirstSlidePanelVL.MinWidth + FrameStatus.Width) * 1.1;
            double minWidth2 = SystemParameters.PrimaryScreenWidth * 6 / 10;
            MinWidth = Math.Max(minWidth1, minWidth2);

            Debug.WriteLine("MinWidth ==> " + MinWidth);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: RootWindow_ContentRendered{NL}{ex.Message}");
        }
    }

    private void Exit_Button_Click(object sender, RoutedEventArgs e)
    {
        string ss = "vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.worKERs.dEV:2083?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.worKERs.dEV&security=tls&alpn=h2&fp=android&fragment=tlshello%2C20-50%2C10-20&sni=s.msmh.worKERs.dEV#1-vless-worker-s.msmh.worKERs.dEV";
        ss = "vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.WORKeRS.dev:8443?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.WORKeRS.dev&security=tls&alpn=h2&fp=android&fragment=tlshello%2C2-4%2C3-5&noise=auto&sni=s.msmh.WORKeRS.dev#1-vless-worker-s.msmh.WORKeRS.dev";
        //vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.WorkeRs.dEv:2083?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.WorkeRs.dEv&security=tls&alpn=http%2F1.1&fp=randomized&fragment=tlshello%2C10-20%2C10-20&sni=s.msmh.WorkeRs.dEv#2-vless-worker-s.msmh.WorkeRs.dEv
        //ss = "vless://UUID@google.com/?fragment=tlshello,2-4,5-11";

        XrayConfig config = ConfigBuilder.Build_Serverless();
        
        string json = JsonTool.Serialize(config);

        Debug.WriteLine(json);
        
        return;

        //string input = "My Custom Input. This Is My Custom Input Asshole! This Is My Custom Input Asshole! This Is My Custom Input Asshole!";
        //WpfInputBox.Show(ref input, "Enter DNS Address:", true);
        //Debug.WriteLine(input);

        //MessageBoxResult mbr = WpfMessageBox.Show("The Message Box");
        //Debug.WriteLine(mbr);


        AppTheme.SwitchTheme(Application.Current.MainWindow, Application.Current.Resources.MergedDictionaries);

        

        //List<Control> listC = Controllers.GetAllControls(this);
        //Debug.WriteLine("++++++++++++++++++++ Count: " + listC.Count);
        //foreach (Control c in listC)
        //{
        //    Debug.WriteLine(c.Name);

        //}


        //AppSettings.AddAllSettings();
        //await AppSettings.SaveAsync(SettingsXmlPath);
        //Debug.WriteLine("YYYYYYYYYYYYYYYY " + AppTheme.GetColor(Application.Current.Resources.MergedDictionaries, AppTheme.DodgerBlueBrush));

    }

    private void View_Button_Click(object sender, RoutedEventArgs e)
    {
        MainSplitPanel.ToggleFly();
    }

    private void QC_ManageServers_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ManageServersWindow msw = new();
            msw.Closed -= Msw_Closed;
            msw.Closed += Msw_Closed;
            Window? owner = this.GetParentWindow();
            if (owner != null) msw.Owner = owner;
            msw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ServersManager.UI_Window = msw;
            msw.ShowDialog();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Msw_Closed(object? sender, EventArgs e)
    {
        if (sender is ManageServersWindow msw)
        {
            try
            {
                // Get Selected
                EnumsAndStructs.GroupItem gi = new();
                if (QC_DnsGroup_ComboBox.SelectedItem is EnumsAndStructs.GroupItem groupItem) gi = groupItem;

                // Bind
                List<EnumsAndStructs.GroupItem> updatedGIs = ServersManager.Get_GroupItems(true);
                QC_DnsGroup_ComboBox.ItemsSource = updatedGIs;

                // Restore Selected
                bool exist = false;
                for (int n = 0; n < updatedGIs.Count; n++)
                {
                    EnumsAndStructs.GroupItem updatedGI = updatedGIs[n];
                    if (updatedGI.Name.Equals(gi.Name))
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist) QC_DnsGroup_ComboBox.SelectedItem = gi;
                else QC_DnsGroup_ComboBox.SelectedIndex = 0;

                // Unsubscribe Event
                msw.Closed -= Msw_Closed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: ManageServersWindow Closed{NL}{ex.Message}");
            }
        }
    }

    private async void QC_UpdateNIC_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Update NIC
            var updateNIC = await SetDnsOnNic_.UpdateNICs(IPAddress.Parse("8.8.8.8"), 53, selectAuto: true);
            QC_NIC_ComboBox.ItemsSource = updateNIC.AllNICs;
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void QC_FindActiveNIC_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Find Active NIC
            var updateNIC = await SetDnsOnNic_.UpdateNICs(IPAddress.Parse("8.8.8.8"), 53, selectAuto: false);
            QC_NIC_ComboBox.ItemsSource = updateNIC.AllNICs;
            QC_NIC_ComboBox.SelectedItem = updateNIC.PrimaryNIC;
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }






    private void Flyout_Info_DnsAddresses_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Info_SetDnsTo.IsOpen = false;
            Flyout_Info_AntiDpiMethods.IsOpen = false;
            Flyout_Info_Upstreams.IsOpen = false;
        }
    }

    private void Flyout_Info_SetDnsTo_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Info_DnsAddresses.IsOpen = false;
            Flyout_Info_AntiDpiMethods.IsOpen = false;
            Flyout_Info_Upstreams.IsOpen = false;
        }
    }

    private void Flyout_Info_AntiDpiMethods_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Info_DnsAddresses.IsOpen = false;
            Flyout_Info_SetDnsTo.IsOpen = false;
            Flyout_Info_Upstreams.IsOpen = false;
        }
    }

    private void Flyout_Info_Upstreams_FlyoutChanged(object sender, WpfFlyoutGroupBox.FlyoutChangedEventArgs e)
    {
        if (e.IsFlyoutOpen)
        {
            Flyout_Info_DnsAddresses.IsOpen = false;
            Flyout_Info_SetDnsTo.IsOpen = false;
            Flyout_Info_AntiDpiMethods.IsOpen = false;
        }
    }

    
}