namespace MsmhToolsClass.MsmhProxyServer;

public class Stats
{

    private DateTime ReceivedLastRead_ = DateTime.Now;
    private DateTime SentLastRead_ = DateTime.Now;

    public Stats() { }

    public double BandwidthReceived { get; private set; }

    public string BandwidthReceivedHumanRead
    {
        get => ConvertTool.ConvertByteToHumanRead(BandwidthReceived);
    }

    public double BandwidthSent { get; private set; }

    public string BandwidthSentHumanRead
    {
        get => ConvertTool.ConvertByteToHumanRead(BandwidthSent);
    }

    public double DownloadSpeedPerSecond { get; set; }

    public string DownloadSpeedPerSecondHumanRead
    {
        get
        {
            double len = DownloadSpeedPerSecond / (DateTime.Now - ReceivedLastRead_).TotalSeconds;
            DownloadSpeedPerSecond = 0;
            ReceivedLastRead_ = DateTime.Now;
            return $"{ConvertTool.ConvertByteToHumanRead(len)}/s";
        }
    }

    public double UploadSpeedPerSecond { get; set; }
    
    public string UploadSpeedPerSecondHumanRead
    {
        get
        {
            double len = UploadSpeedPerSecond / (DateTime.Now - SentLastRead_).TotalSeconds;
            UploadSpeedPerSecond = 0;
            SentLastRead_ = DateTime.Now;
            return $"{ConvertTool.ConvertByteToHumanRead(len)}/s";
        }
    }

    public void AddBytes(int bytes, ByteType byteType)
    {
        if (byteType != ByteType.Sent)
        {
            DownloadSpeedPerSecond += Convert.ToDouble(bytes);
            BandwidthReceived += Convert.ToDouble(bytes);
            return;
        }

        UploadSpeedPerSecond += Convert.ToDouble(bytes);
        BandwidthSent += Convert.ToDouble(bytes);
    }

}