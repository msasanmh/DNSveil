using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.13
// https://www.tcpipguide.com/free/t_DNSMessageResourceRecordFieldFormats-4.htm
public class SoaRecord : ResourceRecord
{
    public string PrimaryNameServer { get; private set; } = string.Empty;
    /// <summary>
    /// Authorities Mailbox (Use . instead of @)
    /// </summary>
    public string ResponsibleAuthoritiesMailbox { get; private set; } = string.Empty;
    public uint SerialNumber { get; private set; }
    public uint RefreshInterval { get; private set; }
    public uint RetryInterval { get; private set; }
    public uint ExpireLimit { get; private set; }
    public uint MinTtl { get; private set; }

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        result += $"{nameof(PrimaryNameServer)}: {PrimaryNameServer}\n";
        result += $"{nameof(ResponsibleAuthoritiesMailbox)}: {ResponsibleAuthoritiesMailbox}\n";
        result += $"{nameof(SerialNumber)}: {SerialNumber}\n";
        result += $"{nameof(RefreshInterval)}: {RefreshInterval}\n";
        result += $"{nameof(RetryInterval)}: {RetryInterval}\n";
        result += $"{nameof(ExpireLimit)}: {ExpireLimit}\n";
        result += $"{nameof(MinTtl)}: {MinTtl}\n";
        return result;
    }

    public SoaRecord() { }

    public SoaRecord(string domain, uint ttl, string primaryNameServer,
                                              string responsibleAuthoritiesMailbox,
                                              uint serialNumber,
                                              uint refreshInterval,
                                              uint retryInterval,
                                              uint expireLimit,
                                              uint minTtl)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.SOA;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        PrimaryNameServer = primaryNameServer;
        ResponsibleAuthoritiesMailbox = responsibleAuthoritiesMailbox;
        SerialNumber = serialNumber;
        RefreshInterval = refreshInterval;
        RetryInterval = retryInterval;
        ExpireLimit = expireLimit;
        MinTtl = minTtl;
    }

    public SoaRecord(ResourceRecord resourceRecord, string primaryNameServer,
                                                    string responsibleAuthoritiesMailbox,
                                                    uint serialNumber,
                                                    uint refreshInterval,
                                                    uint retryInterval,
                                                    uint expireLimit,
                                                    uint minTtl)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        PrimaryNameServer = primaryNameServer;
        ResponsibleAuthoritiesMailbox = responsibleAuthoritiesMailbox;
        SerialNumber = serialNumber;
        RefreshInterval = refreshInterval;
        RetryInterval = retryInterval;
        ExpireLimit = expireLimit;
        MinTtl = minTtl;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos)
    {
        try
        {
            string priNS = ReadRecordName(buffer, pos, out int length).ToString();
            if (string.IsNullOrEmpty(priNS)) return new SoaRecord();
            pos += length;

            string ram = ReadRecordName(buffer, pos, out length, true).ToString();
            if (string.IsNullOrEmpty(ram)) return new SoaRecord();
            pos += length;

            ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint serial);
            pos += 4;

            ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint refresh);
            pos += 4;

            ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint retry);
            pos += 4;

            ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint expire);
            pos += 4;

            ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint ttl);
            pos += 4;
            
            return new SoaRecord(resourceRecord, priNS, ram, serial, refresh, retry, expire, ttl);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS SoaRecord Parse: " + ex.Message);
            return new SoaRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, DnsMessage dnsMessage, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not SoaRecord soaRecord) return false;
            byte[] priNS = WriteRecordName(dnsMessage, soaRecord.PrimaryNameServer, pos + 2);
            byte[] ram = WriteRecordName(dnsMessage, soaRecord.ResponsibleAuthoritiesMailbox, pos + 2 + priNS.Length);
            ByteArrayTool.TryConvertUInt32ToBytes(soaRecord.SerialNumber, out byte[] serial);
            ByteArrayTool.TryConvertUInt32ToBytes(soaRecord.RefreshInterval, out byte[] refresh);
            ByteArrayTool.TryConvertUInt32ToBytes(soaRecord.RetryInterval, out byte[] retry);
            ByteArrayTool.TryConvertUInt32ToBytes(soaRecord.ExpireLimit, out byte[] expire);
            ByteArrayTool.TryConvertUInt32ToBytes(soaRecord.MinTtl, out byte[] ttl);

            int len = priNS.Length + ram.Length + serial.Length + refresh.Length + retry.Length + expire.Length + ttl.Length;
            bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(len), out byte[] rdLength); // 2 Bytes
            if (!rdLengthBool) return false;
            bufferList.AddRange(rdLength);
            bufferList.AddRange(priNS);
            bufferList.AddRange(ram);
            bufferList.AddRange(serial);
            bufferList.AddRange(refresh);
            bufferList.AddRange(retry);
            bufferList.AddRange(expire);
            bufferList.AddRange(ttl);
            pos += 2;
            pos += len;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS SoaRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}