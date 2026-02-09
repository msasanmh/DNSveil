using DNSveil.Logic;
using DNSveil.Logic.DnsServers;
using DNSveil.Logic.UpstreamServers;
using DNSveil.ManageRules;
using DNSveil.ManageDns;
using DNSveil.ManageUpstream;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsClass.V2RayConfigTool;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
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

    public static readonly DnsServersManager DnsManager = new();
    public static readonly UpstreamServersManager UpstreamManager = new();
    public static readonly AgnosticProgram.Rules Rules = new();
    private SetDnsOnNic SetDnsOnNic_ = new();
    private Interface Interface_ = new();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        // Set Main Page
        //FrameMain.NavigationService.Navigate(new Uri("PageMain.xaml", UriKind.Relative));

        // Create UserData Structure
        FileDirectory.CreateEmptyDirectory(Pathes.BinaryDir);
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
            // Load DNS Servers Manager
            bool isInitialized = await DnsManager.InitializeAsync(Pathes.DnsServers_BuiltIn, Pathes.DnsServers_User, Application.Current.MainWindow);
            Debug.WriteLine("DNS Servers Manager Initialized: " + isInitialized);
            if (!isInitialized)
            {
                string msg = "Failed To Initialize DNS Servers JSON File.";
                WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            
            // Load Upstream Servers Manager
            isInitialized = await UpstreamManager.InitializeAsync(Pathes.UpstreamServers_BuiltIn, Pathes.UpstreamServers_User, Application.Current.MainWindow);
            Debug.WriteLine("Upstream Servers Manager Initialized: " + isInitialized);
            if (!isInitialized)
            {
                string msg = "Failed To Initialize Upstream Servers JSON File.";
                WpfMessageBox.Show(this, msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // Bind DnsGroup ComboBox
            QC_DnsGroup_ComboBox.ItemsSource = DnsManager.Get_Groups(true);
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
            QC_UpstreamGroup_ComboBox.ItemsSource = UpstreamManager.Get_Groups(true);
            QC_UpstreamGroup_ComboBox.DisplayMemberPath = "Name";
            QC_UpstreamGroup_ComboBox.SelectedValuePath = "Name";
            QC_UpstreamGroup_ComboBox.SelectedIndex = 0;
            QC_UpstreamGroup_ComboBox.MinWidth = QC_UpstreamGroup_ComboBox.ActualWidth * 1.28;
            QC_UpstreamGroup_ComboBox.MaxWidth = QC_UpstreamGroup_ComboBox.ActualWidth * 1.28;

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

    private async void Exit_Button_Click(object sender, RoutedEventArgs e)
    {
        string sub = "https://raw.githubusercontent.com/MahsaNetConfigTopic/config/refs/heads/main/xray_final.txt";
        List<string> allLinks = await WebAPI.GetLinesFromTextLinkAsync(sub, 5000, CancellationToken.None).ConfigureAwait(false);

        //Debug.WriteLine(allLinks.ToString(Environment.NewLine));
        Debug.WriteLine("Count: " + allLinks.Count);

        string testUrl = "https://captive.apple.com/hotspot-detect.html";
        string fragmentStr = "tlshello,2-4,3-5";



        string ss = "vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.worKERs.dEV:2083?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.worKERs.dEV&security=tls&alpn=h2&fp=android&fragment=tlshello%2C20-50%2C10-20&sni=s.msmh.worKERs.dEV#1-vless-worker-s.msmh.worKERs.dEV";
        ss = "vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.WORKeRS.dev:8443?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.WORKeRS.dev&security=tls&alpn=h2&fp=android&fragment=tlshello%2C2-4%2C3-5&noise=auto&sni=s.msmh.WORKeRS.dev#1-vless-worker-s.msmh.WORKeRS.dev";
        //vless://05f96a64-8f47-5f0c-8e0f-da766f26d6c3@s.msmh.WorkeRs.dEv:2083?encryption=none&type=ws&path=vless-ws%2F%3Fed%3D2048&host=s.msmh.WorkeRs.dEv&security=tls&alpn=http%2F1.1&fp=randomized&fragment=tlshello%2C10-20%2C10-20&sni=s.msmh.WorkeRs.dEv#2-vless-worker-s.msmh.WorkeRs.dEv
        //ss = "vless://1234567890@address.net:8080?alpn=h2%2Chttp%2F1.1&fp=firefox&type=xhttp&sni=xhttpSni.net%2C%20xhttpSni2.net&sid=ShortID&mode=packet-up&path=%2FxhttpPath&security=reality&pqv=Mldsa65Verify&encryption=none&extra=%7B%0A%20%20%22headers%22%3A%20%7B%0A%20%20%20%20%22key%22%3A%20%22value%22%0A%20%20%20%20%7D%0A%7D&pbk=PublicKey&host=xhttpHost.net&spx=SpiderX#Fake%20Vless%20Test";
        //ss = "vmess://eyJhZGQiOiJhZGRyZXNzLm5ldCIsImFpZCI6IjAiLCJhbHBuIjoiIiwiZnAiOiIiLCJob3N0Ijoid3NIb3N0Lm5ldCIsImlkIjoiMTIzNDU2Nzg5MCIsIm5ldCI6IndzIiwicGF0aCI6Ii93c1BhdGgiLCJwb3J0IjoiODA4MCIsInBzIjoiRmFrZSBWbWVzcyIsInNjeSI6ImNoYWNoYTIwLXBvbHkxMzA1Iiwic25pIjoiIiwidGxzIjoiIiwidHlwZSI6Ii0tLSIsInYiOiIyIn0=";
        //ss = "ss://MjAyMi1ibGFrZTMtY2hhY2hhMjAtcG9seTEzMDU6UGFzc1cwcmQ%3D@address.net:8080#Fake%20Shadowsocks";
        //ss = "trojan://PassW0rd@address.net:8080?security=tls&alpn=h3%2Ch2%2Chttp%2F1.1&host=tcpHost.net&headerType=none&fp=edge&type=tcp&sni=tlsSni.net#Fake%20Trojan";
        //ss = "wireguard://secretKey@address.net:8181?address=172.16.0.2%2F32%2C%20192.168.1.0%2F24&presharedkey=preSharedKey&reserved=0%2C0%2C0&publickey=publicKey&mtu=1420#Fake%20Wireguard";
        //ss = "http://MyUser:MyPassw0rd@address.net:8080?email=example@gmail.com#Fake%20Http";
        //ss = "socks://TXlVc2VyOlBhc3N3MHJk@address.net:8080?email=example@gmail.com#Fake%20Socks";

        ss = "vless://90cd4a77-141a-43c9-991b-08263cfe9c10@104.16.134.229:443?path=%2F%3Fed%3D2560&security=tls&encryption=none&host=azadnet6-d7j.pages.dev&fp=random&type=ws&sni=azadnet6-d7j.pages.dev#worker";
        //XrayConfig config = ConfigBuilder.Build_Serverless();
        XrayConfig config = ConfigBuilder.Build(ss, 8053, 8080, "tlshello,2-4,3-5", IPAddress.Parse("8.8.8.8"), 53, false, "https://every1dns.com/dns-query");

        Stopwatch sw = Stopwatch.StartNew();
        string json = await ConfigBuilder.BuildJsonAsync(config);
        sw.Stop();

        string filePath = "D:\\eXe_Dev-GUI\\Proxy\\CMD_Xray\\test.json";
        await File.WriteAllTextAsync(filePath, json);

        ConfigBuilder.GetConfigInfo info = new(json);
        Debug.WriteLine(info.ToString());
        Debug.WriteLine(sw.ElapsedMilliseconds);

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

    private void QC_ManageDnss_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ManageDnsWindow mdw = new();
            mdw.Closed -= Mdw_Closed;
            mdw.Closed += Mdw_Closed;
            Window? owner = this.GetParentWindow();
            if (owner != null) mdw.Owner = owner;
            mdw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DnsManager.UI_Window = mdw;
            mdw.ShowDialog();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Mdw_Closed(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ManageDnsWindow mdw) return;

            // Get Selected
            DnsModel.DnsGroup group = new();
            if (QC_DnsGroup_ComboBox.SelectedItem is DnsModel.DnsGroup groupOut) group = groupOut;

            // Bind
            List<DnsModel.DnsGroup> updatedGroups = DnsManager.Get_Groups(true);
            QC_DnsGroup_ComboBox.ItemsSource = updatedGroups;

            // Restore Selected
            bool exist = false;
            for (int n = 0; n < updatedGroups.Count; n++)
            {
                DnsModel.DnsGroup updatedGroup = updatedGroups[n];
                if (updatedGroup.Name.Equals(group.Name))
                {
                    exist = true;
                    break;
                }
            }
            if (exist) QC_DnsGroup_ComboBox.SelectedItem = group;
            else QC_DnsGroup_ComboBox.SelectedIndex = 0;

            // Unsubscribe Event
            mdw.Closed -= Mdw_Closed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: ManageDnsWindow Closed{NL}{ex.Message}");
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

    private void QC_ManageUpstreams_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ManageUpstreamWindow muw = new();
            muw.Closed -= Muw_Closed;
            muw.Closed += Muw_Closed;
            Window? owner = this.GetParentWindow();
            if (owner != null) muw.Owner = owner;
            muw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UpstreamManager.UI_Window = muw;
            muw.ShowDialog();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Muw_Closed(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ManageUpstreamWindow muw) return;

            // Get Selected
            UpstreamModel.UpstreamGroup group = new();
            if (QC_UpstreamGroup_ComboBox.SelectedItem is UpstreamModel.UpstreamGroup groupOut) group = groupOut;

            // Bind
            List<UpstreamModel.UpstreamGroup> updatedGroups = UpstreamManager.Get_Groups(true);
            QC_UpstreamGroup_ComboBox.ItemsSource = updatedGroups;

            // Restore Selected
            bool exist = false;
            for (int n = 0; n < updatedGroups.Count; n++)
            {
                UpstreamModel.UpstreamGroup updatedGroup = updatedGroups[n];
                if (updatedGroup.Name.Equals(group.Name))
                {
                    exist = true;
                    break;
                }
            }
            if (exist) QC_UpstreamGroup_ComboBox.SelectedItem = group;
            else QC_UpstreamGroup_ComboBox.SelectedIndex = 0;

            // Unsubscribe Event
            muw.Closed -= Muw_Closed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: ManageUpstreamWindow Closed{NL}{ex.Message}");
        }
    }

    private void QC_Rules_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ManageRulesWindow mrw = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mrw.Closed -= Mrw_Closed;
            mrw.Closed += Mrw_Closed;
            mrw.ShowDialog();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(this, ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Mrw_Closed(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not ManageRulesWindow mrw) return;
            await Rules.SaveAsync();
            mrw.Closed -= Mrw_Closed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR: ManageRulesWindow Closed{NL}{ex.Message}");
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