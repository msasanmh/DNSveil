using System.Data;
using System.Text;
using CustomControls;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using MsmhToolsClass;
using System.Globalization;

namespace SecureDNSClient;

public partial class FormDnsScannerExport : Form
{
    // AgnosticSettings XML path
    private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlDnsScannerExport;
    private readonly Settings AppSettings;
    private List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool, bool, bool>>> ExportList = new();
    private readonly List<string> ResultList = new();
    private bool IsExiting { get; set; } = false;
    private bool IsExitDone { get; set; } = false;

    public FormDnsScannerExport(List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool, bool, bool>>> exportList)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        // Text
        Text = "Export Dns Scanner Data (Only Online Servers)";

        // Initialize and load AgnosticSettings
        if (File.Exists(SettingsXmlPath) && XmlTool.IsValidFile(SettingsXmlPath))
            AppSettings = new(this, SettingsXmlPath);
        else
            AppSettings = new(this);

        ExportList = exportList;

        FixScreenDpi();
    }

    private async void FixScreenDpi()
    {
        // Setting Width Of Controls
        await ScreenDPI.SettingWidthOfControls(this);

        // Fix Controls Location
        int spaceBottom = 10, spaceRight = 12, spaceV = 12, spaceH = 16;
        CustomLabelFilterExport.Location = new Point(spaceRight, spaceBottom);

        CustomCheckBoxSecureOnline.Left = CustomLabelFilterExport.Left;
        CustomCheckBoxSecureOnline.Top = CustomLabelFilterExport.Bottom + spaceV;

        CustomCheckBoxInsecureOnline.Left = CustomCheckBoxSecureOnline.Right + (spaceH * 5);
        CustomCheckBoxInsecureOnline.Top = CustomCheckBoxSecureOnline.Top;

        spaceV = 6;
        CustomCheckBoxGoogleSafeSearchActive.Left = spaceRight;
        CustomCheckBoxGoogleSafeSearchActive.Top = CustomCheckBoxSecureOnline.Bottom + spaceV;

        CustomCheckBoxBingSafeSearchActive.Left = CustomCheckBoxInsecureOnline.Left;
        CustomCheckBoxBingSafeSearchActive.Top = CustomCheckBoxGoogleSafeSearchActive.Top;

        CustomCheckBoxYoutubeRestrictActive.Left = spaceRight;
        CustomCheckBoxYoutubeRestrictActive.Top = CustomCheckBoxGoogleSafeSearchActive.Bottom + spaceV;

        CustomCheckBoxAdultContentFilter.Left = CustomCheckBoxBingSafeSearchActive.Left;
        CustomCheckBoxAdultContentFilter.Top = CustomCheckBoxYoutubeRestrictActive.Top;

        spaceV = 16;
        CustomRadioButtonSortBySecure.Left = spaceRight;
        CustomRadioButtonSortBySecure.Top = CustomCheckBoxAdultContentFilter.Bottom + spaceV;

        CustomRadioButtonSortByInsecure.Left = CustomCheckBoxAdultContentFilter.Left;
        CustomRadioButtonSortByInsecure.Top = CustomRadioButtonSortBySecure.Top;

        spaceV = 10;
        CustomButtonExport.Left = CustomRadioButtonSortByInsecure.Left + (CustomRadioButtonSortByInsecure.Width / 3);
        CustomButtonExport.Top = CustomRadioButtonSortByInsecure.Bottom + spaceV;

        // Client Size
        int w = CustomCheckBoxBingSafeSearchActive.Right + (spaceRight * 3);
        int h = CustomButtonExport.Bottom + (spaceBottom * 3);
        ClientSize = new(w, h);
    }

    private async void CustomButtonExport_Click(object sender, EventArgs e)
    {
        // Clear ResultList
        ResultList.Clear();

        // Remove dups
        ExportList = ExportList.Distinct().ToList();

        // Sort
        if (CustomRadioButtonSortBySecure.Checked)
            ExportList = ExportList.OrderBy(x => x.Item4).ToList();
        else
            ExportList = ExportList.OrderBy(x => x.Item6).ToList();

        // Filter
        int count = 0;
        for (int n = 0; n < ExportList.Count; n++)
        {
            var tuple = ExportList[n];
            string secureAddress = tuple.Item1;
            string insecureAddress = tuple.Item2;
            bool secureOnline = tuple.Item3;
            bool insecureOnline = tuple.Item5;
            bool isGoogleSafeSearchActive = tuple.Item7.Item1;
            bool isBingSafeSearchActive = tuple.Item7.Item2;
            bool isYoutubeRestrictActive = tuple.Item7.Item3;
            bool isAdultContentFilter = tuple.Item7.Item4;

            bool mainState1 = (secureOnline && CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                              (!secureOnline && !CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                              (secureOnline || !secureOnline) && CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState2 = (insecureOnline && CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                              (!insecureOnline && !CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                              (insecureOnline || !insecureOnline) && CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState3 = (isGoogleSafeSearchActive && CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (!isGoogleSafeSearchActive && !CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (isGoogleSafeSearchActive || !isGoogleSafeSearchActive) && CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState4 = (isBingSafeSearchActive && CustomCheckBoxBingSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (!isBingSafeSearchActive && !CustomCheckBoxBingSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (isBingSafeSearchActive || !isBingSafeSearchActive) && CustomCheckBoxBingSafeSearchActive.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState5 = (isYoutubeRestrictActive && CustomCheckBoxYoutubeRestrictActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (!isYoutubeRestrictActive && !CustomCheckBoxYoutubeRestrictActive.CheckState.HasFlag(CheckState.Checked)) ||
                              (isYoutubeRestrictActive || !isYoutubeRestrictActive) && CustomCheckBoxYoutubeRestrictActive.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState6 = (isAdultContentFilter && CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Checked)) ||
                              (!isAdultContentFilter && !CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Checked)) ||
                              (isAdultContentFilter || !isAdultContentFilter) && CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Indeterminate);

            bool mainState = mainState1 && mainState2 && mainState3 && mainState4 && mainState5 && mainState6;

            bool secureStateOnline = !string.IsNullOrEmpty(secureAddress) && secureOnline;
            bool insecureStateOnline = !string.IsNullOrEmpty(insecureAddress) && insecureOnline;

            if (mainState && secureStateOnline)
            {
                if (!ResultList.IsContain(secureAddress))
                {
                    ResultList.Add(secureAddress);
                    count++;
                }
            }

            if (mainState && insecureStateOnline)
            {
                if (!ResultList.IsContain(insecureAddress))
                {
                    ResultList.Add(insecureAddress);
                    count++;
                }
            }
        }

        // Message
        if (count == 0)
        {
            string msg = "There is nothing to export.";
            CustomMessageBox.Show(this, msg, "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        // Result
        string result = string.Empty;

        for (int i = 0; i < ResultList.Count; i++)
        {
            string address = ResultList[i];
            result += $"{address}{Environment.NewLine}";
        }

        // Trim
        result = result.TrimEnd(Environment.NewLine);

        // Open Save File Dialog
        using SaveFileDialog sfd = new();
        sfd.Filter = "Custom Servers (*.txt)|*.txt";
        sfd.DefaultExt = ".txt";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;
        sfd.FileName = "sdc_dns_scanner_export_servers";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                await File.WriteAllTextAsync(sfd.FileName, result, new UTF8Encoding(false));
                string msg = "Servers exported successfully.";
                CustomMessageBox.Show(this, msg, "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Something went wrong!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void FormDnsScannerExport_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!IsExiting)
        {
            e.Cancel = true;
            IsExiting = true;

            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "CheckState");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");

            // Add AgnosticSettings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application AgnosticSettings
            await AppSettings.SaveAsync(SettingsXmlPath);
            IsExitDone = true;

            e.Cancel = false;
            Close();
        }
        else
        {
            if (!IsExitDone)
            {
                e.Cancel = true;
                return;
            }

            Dispose();
        }
    }
}