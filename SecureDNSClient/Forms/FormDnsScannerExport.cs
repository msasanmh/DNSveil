using System;
using System.Data;
using System.Text;
using CustomControls;
using System.Diagnostics;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using MsmhToolsClass;

namespace SecureDNSClient
{
    public partial class FormDnsScannerExport : Form
    {
        // Settings XML path
        private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlDnsScannerExport;
        private readonly Settings AppSettings;
        private List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool>>> ExportList = new();

        public FormDnsScannerExport(List<Tuple<string, string, bool, int, bool, int, Tuple<bool, bool>>> exportList)
        {
            // Fix Screed DPI
            ScreenDPI.FixDpiBeforeInitializeComponent(this);
            InitializeComponent();
            ScreenDPI.FixDpiAfterInitializeComponent(this);

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);

            // Initialize and load Settings
            if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
                AppSettings = new(this, SettingsXmlPath);
            else
                AppSettings = new(this);

            ExportList = exportList;
        }

        private async void CustomButtonExport_Click(object sender, EventArgs e)
        {
            // Remove dups
            ExportList = ExportList.Distinct().ToList();

            // Sort
            if (CustomRadioButtonSortBySecure.Checked)
                ExportList = ExportList.OrderBy(x => x.Item4).ToList();
            else
                ExportList = ExportList.OrderBy(x => x.Item6).ToList();

            // Result
            string result = string.Empty;

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
                bool isAdultContentFilter = tuple.Item7.Item2;

                if (((secureOnline && CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                    (!secureOnline && !CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                    (secureOnline || !secureOnline) && CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Indeterminate)) &&
                    ((insecureOnline && CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                    (!insecureOnline && !CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Checked)) ||
                    (insecureOnline || !insecureOnline) && CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Indeterminate)) &&
                    ((isGoogleSafeSearchActive && CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                    (!isGoogleSafeSearchActive && !CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Checked)) ||
                    (isGoogleSafeSearchActive || !isGoogleSafeSearchActive) && CustomCheckBoxGoogleSafeSearchActive.CheckState.HasFlag(CheckState.Indeterminate)) &&
                    ((isAdultContentFilter && CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Checked)) ||
                    (!isAdultContentFilter && !CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Checked)) ||
                    (isAdultContentFilter || !isAdultContentFilter) && CustomCheckBoxAdultContentFilter.CheckState.HasFlag(CheckState.Indeterminate)))
                {
                    if (!string.IsNullOrEmpty(secureAddress) && CustomCheckBoxSecureOnline.CheckState.HasFlag(CheckState.Checked))
                        result += $"{secureAddress}{Environment.NewLine}";
                    if (!string.IsNullOrEmpty(insecureAddress) && CustomCheckBoxInsecureOnline.CheckState.HasFlag(CheckState.Checked))
                        result += $"{insecureAddress}{Environment.NewLine}";
                    Debug.WriteLine(secureAddress);
                    if (!string.IsNullOrEmpty(secureAddress) || !string.IsNullOrEmpty(insecureAddress)) count++;
                }
            }

            // Message
            if (count == 0)
            {
                string msg = "There is nothing to export.";
                CustomMessageBox.Show(this, msg, "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Trim
            result = result.TrimEnd(Environment.NewLine.ToCharArray());

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
            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "CheckState");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");

            // Add Settings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application Settings
            await AppSettings.SaveAsync(SettingsXmlPath);
        }
    }
}
