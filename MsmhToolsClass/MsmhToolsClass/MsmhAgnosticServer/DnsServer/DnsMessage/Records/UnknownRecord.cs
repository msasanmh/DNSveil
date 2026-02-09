using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class UnknownRecord : ResourceRecord
{
    public byte[] RDDATA { get; private set; } = Array.Empty<byte>();

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        try { result += $"{nameof(RDDATA)}: {BitConverter.ToString(RDDATA)}\n"; } catch (Exception) { }
        return result;
    }

    public UnknownRecord() { }

    public UnknownRecord(ResourceRecord resourceRecord, byte[] rdData)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        RDDATA = rdData;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos, ushort rdLength)
    {
        try
        {
            int count = pos + rdLength;
            if (count <= buffer.Length)
            {
                byte[] rdData = buffer[pos..count];
                return new UnknownRecord(resourceRecord, rdData);
            }
            else return new UnknownRecord();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS UnknownRecord Parse: " + ex.Message);
            return new UnknownRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not ResourceRecord record) return false;

            byte[] name = WriteRecordName(dnsMessage, resourceRecord.Name);
            int lenOfTCT = 8; // TYPE(2), CLASS(2), TTL(4)
            int iRecordLen = name.Length + lenOfTCT;

            //bufferList.Clear(); // We Don't Replace The Whole Record To Be Able Modify TTL, etc.
            byte[] rDataBuffer = record.RecordBuffer[10..];
            bufferList.AddRange(rDataBuffer);

            // Adjust Pos (Just In Case)
            pos += rDataBuffer.Length;

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS UnknownRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}