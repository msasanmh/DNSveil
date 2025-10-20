using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class Stats
{
    public Stats() { }

    public double BandwidthSent { get; private set; }

    public string BandwidthSentHumanRead
    {
        get => ConvertTool.ConvertByteToHumanRead(BandwidthSent);
    }

    public double BandwidthReceived { get; private set; }

    public string BandwidthReceivedHumanRead
    {
        get => ConvertTool.ConvertByteToHumanRead(BandwidthReceived);
    }

    public async Task<string> UploadSpeedAsync()
    {
        try
        {
            double b1 = BandwidthSent;
            await Task.Delay(1000);
            double b2 = BandwidthSent;
            double len = b2 - b1;
            return $"{ConvertTool.ConvertByteToHumanRead(len)}/s";
        }
        catch (Exception)
        {
            return "-1 Byte/s";
        }
    }

    public async Task<string> DownloadSpeedAsync()
    {
        try
        {
            double b1 = BandwidthReceived;
            await Task.Delay(1000);
            double b2 = BandwidthReceived;
            double len = b2 - b1;
            return $"{ConvertTool.ConvertByteToHumanRead(len)}/s";
        }
        catch (Exception)
        {
            return "-1 Byte/s";
        }
    }

    public void AddBytes(int bytes, ByteType byteType)
    {
        try
        {
            bool isDouble = double.TryParse(bytes.ToString(), out double value);
            if (isDouble)
            {
                if (byteType == ByteType.Sent)
                {
                    lock (this)
                    {
                        BandwidthSent += value;
                    }
                }

                if (byteType == ByteType.Received)
                {
                    lock (this)
                    {
                        BandwidthReceived += value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Stats: " + ex.Message);
        }
    }

}