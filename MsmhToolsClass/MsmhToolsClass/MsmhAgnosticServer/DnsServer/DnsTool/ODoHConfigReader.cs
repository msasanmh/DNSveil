using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class ObliviousDoHConfigs
{
    public List<ObliviousDoHConfig> Configs { get; set; } = new();
}

public class ObliviousDoHConfig
{
    public int Version { get; set; }
    public int KemID { get; set; }
    public int KdfID { get; set; }
    public int AeadID { get; set; }
    public byte[] PublicKeyBytes { get; set; } = Array.Empty<byte>();
}

public static class ObliviousDoHConfigParser
{
    public const ushort ODOH_VERSION = 0x0001;

    public static ObliviousDoHConfigs ParseODoHConfigs(byte[] buffer)
    {
        List<ObliviousDoHConfig> configs = new();

        try
        {
            if (buffer.Length < 2) return CreateObliviousDoHConfigs(configs);

            ByteArrayTool.TryConvertBytesToUInt16(buffer[0..2], out ushort length);
            ushort offset = 2;

            while (true)
            {
                var (configVersion, configLength) = ParseConfigHeader(buffer[offset..]);

                if (buffer.Length - offset < configLength) // buffer.Length - offset < configLength
                {
                    Debug.WriteLine($"Invalid Serialized ObliviousDoHConfig, Expected {length} Bytes, Got {buffer.Length - offset}");
                    return CreateObliviousDoHConfigs(configs);
                }

                if (IsSupportedConfigVersion(configVersion))
                {
                    ObliviousDoHConfig? config = ParseODoHConfig(buffer[offset..]);
                    if (config != null) configs.Add(config);
                }

                offset += (ushort)(4 + configLength);

                if (offset >= 2 + length) break; // Stop Reading
            }
        }
        catch (Exception) { }

        return CreateObliviousDoHConfigs(configs);
    }

    private static ObliviousDoHConfig? ParseODoHConfig(byte[] configBuffer)
    {
        try
        {
            (ushort version, int length) = ParseConfigHeader(configBuffer);

            if (!IsSupportedConfigVersion(version))
            {
                Debug.WriteLine($"Unsupported Version: {version}");
                return null;
            }

            if (configBuffer.Length - 4 < length)
            {
                Debug.WriteLine($"Invalid Serialized ObliviousDoHConfig, Expected {length} Bytes, Got {configBuffer.Length - 4}");
                return null;
            }

            byte[] contentBuffer = configBuffer[4..length];
            if (contentBuffer.Length < 8) return null;

            ByteArrayTool.TryConvertBytesToUInt16(contentBuffer[0..2], out ushort kemId);
            ByteArrayTool.TryConvertBytesToUInt16(contentBuffer[2..4], out ushort kdfId);
            ByteArrayTool.TryConvertBytesToUInt16(contentBuffer[4..6], out ushort aeadId);
            ByteArrayTool.TryConvertBytesToUInt16(contentBuffer[6..8], out ushort publicKeyLength);

            if (contentBuffer.Length < 4 + publicKeyLength) return null;

            byte[] publicKeyBytes = contentBuffer[8..publicKeyLength];

            return new ObliviousDoHConfig
            {
                Version = version,
                KemID = kemId,
                KdfID = kdfId,
                AeadID = aeadId,
                PublicKeyBytes = publicKeyBytes
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static (ushort, ushort) ParseConfigHeader(byte[] buffer)
    {
        if (buffer.Length < 4) return (0, 0);
        ByteArrayTool.TryConvertBytesToUInt16(buffer[0..2], out ushort configVersion);
        ByteArrayTool.TryConvertBytesToUInt16(buffer[2..4], out ushort configLength);
        return (configVersion, configLength);
    }

    private static bool IsSupportedConfigVersion(ushort configVersion)
    {
        return configVersion == ODOH_VERSION;
    }

    private static ObliviousDoHConfigs CreateObliviousDoHConfigs(List<ObliviousDoHConfig> configs)
    {
        return new ObliviousDoHConfigs
        {
            Configs = configs
        };
    }
}













public class ObliviousDNSMessageBody
{
    public byte[] DnsMessage { get; set; } = Array.Empty<byte>();
    public byte[] Padding { get; set; } = Array.Empty<byte>();
}

public class ObliviousDNSQuery
{
    public ObliviousDNSMessageBody MessageBody { get; set; } = new();

    public static ObliviousDNSQuery CreateObliviousDNSQuery(byte[] query, ushort paddingBytes)
    {
        var msg = new ObliviousDNSMessageBody
        {
            DnsMessage = query,
            Padding = new byte[paddingBytes]
        };

        return new ObliviousDNSQuery
        {
            MessageBody = msg
        };
    }
}





