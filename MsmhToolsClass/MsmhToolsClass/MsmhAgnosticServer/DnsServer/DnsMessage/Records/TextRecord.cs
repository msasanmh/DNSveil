using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://datatracker.ietf.org/doc/html/rfc1035#section-3.3.14
public class TextRecord : ResourceRecord
{
    public List<string> Texts { get; private set; } = new();
    public List<TXTCertificate> TXTCertificates { get; private set; } = new();

    public enum ESVersion : ushort
    {
        X25519_XSalsa20Poly1305 = 1,
        X25519_XChacha20Poly1305 = 2
    }

    public class TXTCertificate
    {
        public string Magic { get; private set; } = "DNSC";
        public ESVersion Version { get; private set; }
        public ushort MinorVersion { get; private set; }
        /// <summary>
        /// HEX String (128 Char)
        /// </summary>
        public string Signature { get; private set; } = string.Empty;
        /// <summary>
        /// HEX String (64 Char)
        /// </summary>
        public string PublicKey { get; private set; } = string.Empty;
        /// <summary>
        /// HEX String (16 Char)
        /// </summary>
        public string ClientMagic { get; private set; } = string.Empty;
        public uint Serial { get; private set; }
        public uint StartTimeStampInSec { get; private set; }
        public uint EndTimeStampInSec { get; private set; }

        public static TXTCertificate Read(byte[] buffer)
        {
            TXTCertificate txtCertificate = new();
            if (buffer.Length != 124) return txtCertificate;

            try
            {
                int pos = 0;
                txtCertificate.Magic = Encoding.UTF8.GetString(buffer[pos..(pos + 4)]);
                pos += 4;
                ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort version);
                txtCertificate.Version = (ESVersion)Enum.Parse(typeof(ESVersion), version.ToString());
                pos += 2;
                ByteArrayTool.TryConvertBytesToUInt16(buffer[pos..(pos + 2)], out ushort minorVersion);
                pos += 2;
                txtCertificate.MinorVersion = minorVersion;
                txtCertificate.Signature = Convert.ToHexString(buffer[pos..(pos + 64)]).ToLower();
                pos += 64;
                txtCertificate.PublicKey = Convert.ToHexString(buffer[pos..(pos + 32)]).ToLower();
                pos += 32;
                txtCertificate.ClientMagic = Convert.ToHexString(buffer[pos..(pos + 8)]).ToLower();
                pos += 8;
                ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint serial);
                txtCertificate.Serial = serial;
                pos += 4;
                ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint startTimeStampInSec);
                txtCertificate.StartTimeStampInSec = startTimeStampInSec;
                pos += 4;
                ByteArrayTool.TryConvertBytesToUInt32(buffer[pos..(pos + 4)], out uint endTimeStampInSec);
                txtCertificate.EndTimeStampInSec = endTimeStampInSec;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DNS TextRecord TXTCertificate Read: " + ex.Message);
            }

            return txtCertificate;
        }

        public static bool TryWrite(TXTCertificate txtCertificate, out byte[] buffer)
        {
            try
            {
                List<byte> bufferList = new();
                bufferList.AddRange(Encoding.UTF8.GetBytes(txtCertificate.Magic));
                ByteArrayTool.TryConvertUInt16ToBytes((ushort)txtCertificate.Version, out byte[] version);
                bufferList.AddRange(version);
                ByteArrayTool.TryConvertUInt16ToBytes(txtCertificate.MinorVersion, out byte[] minorVersion);
                bufferList.AddRange(minorVersion);
                bufferList.AddRange(Convert.FromHexString(txtCertificate.Signature));
                bufferList.AddRange(Convert.FromHexString(txtCertificate.PublicKey));
                bufferList.AddRange(Convert.FromHexString(txtCertificate.ClientMagic));
                ByteArrayTool.TryConvertUInt32ToBytes(txtCertificate.Serial, out byte[] serial);
                bufferList.AddRange(serial);
                ByteArrayTool.TryConvertUInt32ToBytes(txtCertificate.StartTimeStampInSec, out byte[] startTimeStampInSec);
                bufferList.AddRange(startTimeStampInSec);
                ByteArrayTool.TryConvertUInt32ToBytes(txtCertificate.EndTimeStampInSec, out byte[] endTimeStampInSec);
                bufferList.AddRange(endTimeStampInSec);
                buffer = bufferList.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DNS TextRecord TXTCertificate TryWrite: " + ex.Message);
                buffer = Array.Empty<byte>();
                return false;
            }
        }
    }

    public override string ToString()
    {
        string result = base.ToString() + "\n";
        for (int i = 0; i < Texts.Count; i++)
        {
            string text = Texts[i];
            result += $"Text Number {i + 1}: {text}\n";
        }
        for (int j = 0; j < TXTCertificates.Count; j++)
        {
            TXTCertificate cert = TXTCertificates[j];
            result += $"Certificate Number {j + 1}:\n";
            result += $"{nameof(cert.Magic)}: {cert.Magic}\n";
            result += $"{nameof(cert.Version)}: {cert.Version}\n";
            result += $"{nameof(cert.MinorVersion)}: {cert.MinorVersion}\n";
            result += $"{nameof(cert.Signature)}: {cert.Signature}\n";
            result += $"{nameof(cert.PublicKey)}: {cert.PublicKey}\n";
            result += $"{nameof(cert.ClientMagic)}: {cert.ClientMagic}\n";
            result += $"{nameof(cert.Serial)}: {cert.Serial}\n";
            result += $"{nameof(cert.StartTimeStampInSec)}: {cert.StartTimeStampInSec}\n";
            result += $"{nameof(cert.EndTimeStampInSec)}: {cert.EndTimeStampInSec}\n";
        }
        return result;
    }

    public TextRecord() { }

    public TextRecord(string domain, uint ttl, List<string> texts, List<TXTCertificate> txtCertificates)
    {
        Name = domain;
        TYPE = DnsEnums.RRType.TEXT;
        CLASS = DnsEnums.CLASS.IN;
        TimeToLive = ttl;
        TTLDateTime = DateTime.UtcNow;
        Texts = texts;
        TXTCertificates = txtCertificates;
    }

    public TextRecord(ResourceRecord resourceRecord, List<string> texts, List<TXTCertificate> txtCertificates)
    {
        Name = resourceRecord.Name;
        TYPE = resourceRecord.TYPE;
        CLASS = resourceRecord.CLASS;
        TimeToLive = resourceRecord.TimeToLive;
        TTLDateTime = resourceRecord.TTLDateTime;
        Texts = texts;
        TXTCertificates = txtCertificates;
    }

    public static ResourceRecord Parse(ResourceRecord resourceRecord, byte[] buffer, int pos, ushort rLength)
    {
        try
        {
            List<string> texts = new();
            List<TXTCertificate> txtCertificates = new();

            int currentPos = 0;
            while (true)
            {
                int len = buffer[pos];
                pos++;
                currentPos++;

                string magic = Encoding.UTF8.GetString(buffer, pos, 4);
                if (magic.Equals("DNSC"))
                {
                    txtCertificates.Add(TXTCertificate.Read(buffer[pos..(pos + len)]));
                }
                else
                {
                    texts.Add(Encoding.UTF8.GetString(buffer, pos, len));
                }

                pos += len;
                currentPos += len;
                if (currentPos >= rLength) break;
            }
            
            return new TextRecord(resourceRecord, texts, txtCertificates);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS TextRecord Parse: " + ex.Message);
            return new TextRecord();
        }
    }

    public static bool TryWrite(IResourceRecord resourceRecord, List<byte> bufferList, ref int pos)
    {
        try
        {
            // RDLENGTH & RDDATA
            if (resourceRecord is not TextRecord textRecord) return false;

            List<byte> textsList = new();
            for (int i = 0; i < textRecord.Texts.Count; i++)
            {
                string text = textRecord.Texts[i];
                byte[] textArray = Encoding.UTF8.GetBytes(text);
                byte textLen = (byte)textArray.Length;
                textsList.Add(textLen);
                textsList.AddRange(textArray);
            }
            byte[] textsArray = textsList.ToArray();

            List<byte> txtCertificatesList = new();
            for (int j = 0; j < textRecord.TXTCertificates.Count; j++)
            {
                TXTCertificate txtCertificate = textRecord.TXTCertificates[j];
                TXTCertificate.TryWrite(txtCertificate, out byte[] txtCertificateArray);
                byte txtCertificateLen = (byte)txtCertificateArray.Length; // 124
                txtCertificatesList.Add(txtCertificateLen);
                txtCertificatesList.AddRange(txtCertificateArray);
            }
            byte[] txtCertificatesArray = txtCertificatesList.ToArray();

            int len = textsArray.Length + txtCertificatesArray.Length;
            bool rdLengthBool = ByteArrayTool.TryConvertUInt16ToBytes(Convert.ToUInt16(len), out byte[] rdLength); // 2 Bytes
            if (!rdLengthBool) return false;
            bufferList.AddRange(rdLength);
            bufferList.AddRange(textsArray);
            bufferList.AddRange(txtCertificatesArray);
            pos += 2;
            pos += len;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS TextRecord TryWrite: " + ex.Message);
            return false;
        }
    }
}