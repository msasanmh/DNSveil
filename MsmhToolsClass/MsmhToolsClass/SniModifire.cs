using System.Diagnostics;

namespace MsmhToolsClass;

// MAC Must Be Recalculate With The MAC Key
public class SniModifire
{
    public byte[] ModifiedData { get; private set; } = Array.Empty<byte>();

    private readonly int MaxDataLength = 65536;
    private const int SNI_PADDING_PREFIX_LEN = 4;

    public SniModifire(SniReader sniReader, int sniPaddingSize)
    {
        try
        {
            ModifiedData = sniReader.Data;

            if (sniReader.HasSniExtension && sniReader.SniExtensionList.Count > 0)
            {
                // Adjust SNI Padding Size
                int sniPaddingPrefixLen = SNI_PADDING_PREFIX_LEN;
                if (sniPaddingSize < 2)
                {
                    sniPaddingSize = 0;
                    sniPaddingPrefixLen = 0;
                }
                int maxPadding = MaxDataLength - sniReader.Data.Length - sniPaddingPrefixLen;
                if (sniPaddingSize > maxPadding) sniPaddingSize = maxPadding;

                List<byte> modifiedDataList = new();
                int pos = 0;

                int in1 = sniReader.AllLengths.TLS_Record_Layer_StartIndex_2Bytes;
                modifiedDataList.AddRange(sniReader.Data[pos..in1]);
                //Debug.WriteLine($"{pos} => {in1}");
                pos = in1;

                int len1 = sniReader.AllLengths.TLS_Record_Layer_Length + sniPaddingPrefixLen + sniPaddingSize;
                ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(len1), out byte[] len1Bytes);
                modifiedDataList.AddRange(len1Bytes);
                //Debug.WriteLine($"Added {len1Bytes.Length} Bytes");
                pos += 2;

                int in2 = sniReader.AllLengths.Client_Hello_StartIndex_3Bytes;
                modifiedDataList.AddRange(sniReader.Data[pos..in2]);
                //Debug.WriteLine($"{pos} => {in2}");
                pos = in2;

                int len2 = sniReader.AllLengths.Client_Hello_Length + sniPaddingPrefixLen + sniPaddingSize;
                ByteArrayTool.TryConvertUInt24ToBytes(Convert.ToUInt32(len2), out byte[] len2Bytes);
                modifiedDataList.AddRange(len2Bytes);
                //Debug.WriteLine($"Added {len2Bytes.Length} Bytes");
                pos += 3;

                int in3 = sniReader.AllLengths.Extensions_StartIndex_2Bytes;
                modifiedDataList.AddRange(sniReader.Data[pos..in3]);
                //Debug.WriteLine($"{pos} => {in3}");
                pos = in3;

                int len3 = sniReader.AllLengths.Extensions_Length + sniPaddingPrefixLen + sniPaddingSize;
                ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(len3), out byte[] len3Bytes);
                modifiedDataList.AddRange(len3Bytes);
                //Debug.WriteLine($"Added {len3Bytes.Length} Bytes");
                pos += 2;
                //-------------------
                SniReader.SniExtension sniExtension = sniReader.SniExtensionList[^1];

                int in_EndOfSniExtension = sniExtension.StartIndex + sniExtension.Length;
                modifiedDataList.AddRange(sniReader.Data[pos..in_EndOfSniExtension]);
                //Debug.WriteLine($"{pos} => {in_EndOfSniExtension}");
                pos = in_EndOfSniExtension;

                // Add SNI Padding Extension
                byte[] sniPaddingExtBytes = GetPadding(sniPaddingSize);
                modifiedDataList.AddRange(sniPaddingExtBytes);
                //Debug.WriteLine($"Added {sniPaddingExtBytes.Length} Bytes (SNI Padding)");

                // Add The Rest
                modifiedDataList.AddRange(sniReader.Data[pos..]);
                //Debug.WriteLine($"{pos} => {sniReader.Data.Length}");

                ModifiedData = modifiedDataList.ToArray();

                //HMACSHA256 hMac = new();
                //var tt = hMac.ComputeHash
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SniModifire: " + ex.Message);
        }
    }

    // Padding Is A TLS Extension (We Add Padding After SNI Extension)
    private static byte[] GetPadding(int sniPaddingSize)
    {
        if (sniPaddingSize >= 2)
        {
            int padding = SNI_PADDING_PREFIX_LEN + sniPaddingSize;
            byte[] paddingData = new byte[padding];

            // SNI Padding Extension Type (2 bytes)
            paddingData[0] = 0x00;
            paddingData[1] = 0x15;

            // SNI Padding Extension Data Length (2 Bytes Length)
            paddingData[2] = (byte)((sniPaddingSize >> 8) & 0xFF);
            paddingData[3] = (byte)(sniPaddingSize & 0xFF);

            // Add SNI Padding Data Which Is Bunch Of Zeros
            for (int n = 4; n < padding; n++)
            {
                paddingData[n] = 0x00;
            }

            return paddingData;
        }
        else
        {
            return Array.Empty<byte>();
        }
    }

    private byte[] GetMinPadding()
    {
        byte[] paddingData = new byte[4];

        // Padding Extension Type (2 bytes)
        paddingData[0] = 0x00;
        paddingData[1] = 0x15;

        // Padding Extension Data Length (2 bytes length)
        paddingData[2] = 0x00;
        paddingData[3] = 0x00;

        return paddingData;
    }

}