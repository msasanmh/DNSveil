using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MsmhToolsClass;
using MsmhToolsWpfClass;
using MsmhToolsWpfClass.Themes;
using static DNSveil.Logic.DnsServers.DnsModel;

namespace DNSveil.ManageDns;

/// <summary>
/// Interaction logic for MaliciousWindow.xaml
/// </summary>
public partial class MaliciousWindow : WpfWindow
{
    public MaliciousWindow()
    {
        InitializeComponent();

        // Invariant Culture
        Info.SetCulture(CultureInfo.InvariantCulture);
    }

    private void ReadData(bool alsoReadUserData)
    {
        try
        {
            // Get Source URLS
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            List<string> urlsOrFiles = settings.MaliciousDomains.Source_URLs;
            Source_TextBox.DispatchIt(() => Source_TextBox.Text = urlsOrFiles.ToString(Environment.NewLine));

            // Get Server Items
            List<string> serverItems = settings.MaliciousDomains.Items_Server;

            // Set Total Server Items
            Source_Info_TextBlock.Clear();
            Source_Info_TextBlock.AppendText("Total Domains: ", null, $"{serverItems.Count}", Brushes.Orange);

            // Get Green Brush
            Brush greenBrush = AppTheme.GetBrush(AppTheme.MediumSeaGreenBrush);

            // Get UpdateDetails
            UpdateSource_NumericUpDown.DispatchIt(() => UpdateSource_NumericUpDown.Value = settings.MaliciousDomains.UpdateSource);
            UpdateSource_TextBlock.Clear();
            UpdateSource_TextBlock.AppendText("Last Update: ", null, $"{settings.MaliciousDomains.LastUpdateSource:yyyy/MM/dd HH:mm:ss}", greenBrush);

            // Set Server Items To TextBox
            ServerDomains_TextBox.DispatchIt(() => ServerDomains_TextBox.Text = serverItems.ToString(Environment.NewLine));

            if (alsoReadUserData)
            {
                // Get And Set User Items
                List<string> userItems = settings.MaliciousDomains.Items_User;
                UserDomains_TextBox.DispatchIt(() => UserDomains_TextBox.Text = userItems.ToString(Environment.NewLine));

                // Get And Set User Exception Items
                List<string> userExceptionItems = settings.MaliciousDomains.Items_UserException;
                UserExceptionDomains_TextBox.DispatchIt(() => UserExceptionDomains_TextBox.Text = userExceptionItems.ToString(Environment.NewLine));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MaliciousWindow ReadData: " + ex.Message);
        }
    }

    private void WpfWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load Theme


            // Set Max Lines Of TextBoxes
            Source_TextBox.SetMaxLines(3, this);

            // Read Data
            ReadData(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MaliciousWindow WpfWindow_Loaded: " + ex.Message);
        }
    }

    private void WpfWindow_ContentRendered(object sender, EventArgs e)
    {
        try
        {
            // Set SizeToContent To Manual
            SizeToContent = SizeToContent.Manual;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MaliciousWindow WpfWindow_ContentRendered: " + ex.Message);
        }
    }

    private void ChangeControlsState(bool enable)
    {
        this.DispatchIt(() =>
        {
            IsHitTestVisible = enable;
            IsEnabled = enable;
        });
    }

    private async Task<bool> MaliciousDomainsFetchSourceAsync(List<string> urlsOrFiles)
    {
        try
        {
            if (urlsOrFiles.Count == 0) return false;

            List<string> serverDomains = new();
            for (int n = 0; n < urlsOrFiles.Count; n++)
            {
                string urlOrFile = urlsOrFiles[n];
                List<string> domains = await WebAPI.GetLinesFromTextLinkAsync(urlOrFile, 30000, CancellationToken.None);
                serverDomains.AddRange(domains);
            }

            // DeDup
            serverDomains = serverDomains.Distinct().ToList();
            if (serverDomains.Count == 0) return false;

            // Update Settings
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            settings.MaliciousDomains.Items_Server = serverDomains; // Add To MaliciousDomains => ServerItems
            settings.MaliciousDomains.LastUpdateSource = DateTime.Now; // Update LastUpdateSource

            // Save Settings
            await MainWindow.DnsManager.Update_Settings_Async(settings, true);

            ReadData(false);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MaliciousWindow MaliciousDomainsFetchSourceAsync: " + ex.Message);
            return false;
        }
    }

    private async void FetchSource_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not WpfButton b) return;
            b.DispatchIt(() => b.Content = "Fetching...");
            ChangeControlsState(false);

            // Get URLs
            List<string> urlsOrFiles = Source_TextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Update Settings
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();
            settings.MaliciousDomains.Source_URLs = urlsOrFiles; // Save URLs

            // Save Settings
            await MainWindow.DnsManager.Update_Settings_Async(settings, true);

            // Fetch Domains
            bool isSuccess = await MaliciousDomainsFetchSourceAsync(urlsOrFiles);
            if (!isSuccess)
            {
                string msg = "Couldn't Find Any Domain!";
                WpfMessageBox.Show(this, msg);
            }

            b.DispatchIt(() => b.Content = "Fetch Domains");
            ChangeControlsState(true);
        }
        catch (Exception) { }
    }

    private async void SaveClose_Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not WpfButton b) return;
            b.DispatchIt(() => b.Content = "Saving...");
            ChangeControlsState(false);

            // Update Settings
            DnsSettings settings = MainWindow.DnsManager.Get_Settings();

            // Get And Save URLs
            List<string> urlsOrFiles = Source_TextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            settings.MaliciousDomains.Source_URLs = urlsOrFiles;

            // Get And Save UpdateSource_NumericUpDown
            int updateSource = UpdateSource_NumericUpDown.Value.ToInt();
            settings.MaliciousDomains.UpdateSource = updateSource;

            // Get And Save User Domains
            List<string> userDomains = UserDomains_TextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            settings.MaliciousDomains.Items_User = userDomains;
            

            // Get And Save User Exception Domains
            List<string> userExceptionDomains = UserExceptionDomains_TextBox.Text.SplitToLines(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            settings.MaliciousDomains.Items_UserException = userExceptionDomains;

            // Save Settings
            await MainWindow.DnsManager.Update_Settings_Async(settings, true);

            // Close Window
            Close();
        }
        catch (Exception) { }
    }

    private void WpfWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Do Nothing
    }

}