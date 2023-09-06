using System.Diagnostics;

namespace SdcMaui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

        // Android Only
        //if (!OperatingSystem.IsAndroid()) Environment.Exit(0);

        Updater();
        UpdateBoolDns();
        UpdateBoolHttpProxy();
    }

    private async void OnBtnCheckClicked(object sender, EventArgs e)
    {
        bool per = await IsPermitionsGranted();
        if (!per) return;

        StartCheck();
        SemanticScreenReader.Announce(BtnCheck.Text);
    }
    
    private async void OnBtnConnectClicked(object sender, EventArgs e)
	{
        bool per = await IsPermitionsGranted();
        if (!per) return;



        //StartConnect();
        SemanticScreenReader.Announce(BtnConnect.Text);
	}
}

