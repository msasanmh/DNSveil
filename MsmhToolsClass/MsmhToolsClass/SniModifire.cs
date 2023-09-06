using System;
using System.Diagnostics;
using System.Text;

namespace MsmhToolsClass
{
    // https://tls13.xargs.org
    public class SniModifire
    {
        // Needs Certificate
        public class TlsExtensions
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public int StartIndex { get; set; } = -1;
            public int Length { get; set; } = -1;
        }

        public class SniExtension
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public int StartIndex { get; set; } = -1;
            public int Length { get; set; } = -1;
        }

        public class SNI
        {
            public string ServerName { get; set; } = string.Empty;
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public int StartIndex { get; set; } = -1;
            public int Length { get; set; } = -1;
        }

        public int Padding { get; set; } = 0;
        public bool PutSniLast { get; set; } = false;

        public string ReasonPhrase { get; private set; } = string.Empty;
        public bool HasTlsExtensions { get; private set; } = false;
        public TlsExtensions AllExtensions { get; private set; } = new();
        public bool HasSniExtension { get; private set; } = false;
        public List<SniExtension> SniExtensionList { get; private set; } = new();
        public bool HasSni { get; private set; } = false;
        public List<SNI> SniList { get; private set; } = new();
        public byte[] ModifiedData { get; private set; } = Array.Empty<byte>();

        private readonly byte[] Data = Array.Empty<byte>();
        private readonly int MaxDataLength = 65536;
        private const int TLS_HEADER_LEN = 5;
        private const int TLS_HANDSHAKE_CONTENT_TYPE = 0x16;
        private const int TLS_HANDSHAKE_TYPE_CLIENT_HELLO = 0x01;

        public SniModifire(byte[] data)
        {
            Data = data;

            int maxPadding = MaxDataLength - data.Length - 4;
            if (Padding > maxPadding) Padding = maxPadding;

            int pos = TLS_HEADER_LEN;
            int dataLength = data.Length;

            if (dataLength < TLS_HEADER_LEN)
            {
                ReasonPhrase = "TCP payload is not large enough for a TLS header.";
                return;
            }

            // RECORD HEADER
            if (data[0] == 1 & 0x80 == 1 && data[2] == 1)
            {
                ReasonPhrase = "Received SSL 2.0 Client Hello which can not support SNI.";
                return;
            }
            else
            {
                if (data[0] != TLS_HANDSHAKE_CONTENT_TYPE)
                {
                    ReasonPhrase = "Request did not begin with TLS handshake.";
                    return;
                }

                int tls_version_major = data[1];
                int tls_version_minor = data[2];

                if (tls_version_major < 3)
                {
                    ReasonPhrase = $"Received SSL handshake cannot support SNI. Min TLS: {tls_version_minor} Max TLS: {tls_version_major}";
                    return;
                }

                // TLS Record Length (Length of handshake message) (2 bytes length)
                int len = (data[3] << 8) + data[4];
                dataLength = Math.Min(dataLength, len + TLS_HEADER_LEN);

                // Check we received entire TLS record length
                if (dataLength < len + TLS_HEADER_LEN)
                {
                    ReasonPhrase = "Didn't receive entire TLS record length.";
                    return;
                }

                // HANDSHAKE HEADER
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Handshake error.";
                    return;
                }

                // data[5] == 0x01
                if (data[pos] != TLS_HANDSHAKE_TYPE_CLIENT_HELLO)
                {
                    ReasonPhrase = "Not a client hello.";
                    return;
                }

                //== Create Modified Data
                int mPos = 0;
                byte[] mData1 = new byte[pos - mPos];
                Buffer.BlockCopy(data, 0, mData1, 0, mData1.Length);
                mPos += mData1.Length;
                if (Padding > 0)
                {
                    int padding = len + Padding + 4;
                    mData1[mData1.Length - 2] = (byte)(padding >> 8);
                    mData1[mData1.Length - 1] = (byte)padding;

                    int len2 = (mData1[mData1.Length - 2] << 8) + mData1[mData1.Length - 1];
                    Debug.WriteLine(len2);
                }
                ModifiedData = ModifiedData.Concat(mData1).ToArray();

                // Skip Handshake Message Type
                pos += 1;

                // Length of client hello data (3 bytes length)
                len = (data[pos] << 16) + (data[pos + 1] << 8) + data[pos + 2];
                // Skip Length of client hello data
                pos += 3;

                //== Create Modified Data
                byte[] mData2 = new byte[pos - mPos];
                Buffer.BlockCopy(data, mPos, mData2, 0, mData2.Length);
                mPos += mData2.Length;
                if (Padding > 0)
                {
                    int padding = len + Padding + 4;
                    mData2[mData2.Length - 3] = (byte)(padding >> 16); // (byte)(padding >> 16)
                    mData2[mData2.Length - 2] = (byte)(padding >> 8);
                    mData2[mData2.Length - 1] = (byte)padding;

                    int len2 = (mData2[mData2.Length - 3] << 16) + (mData2[mData2.Length - 2] << 8) + mData2[mData2.Length - 1];
                    Debug.WriteLine(len2);
                }
                ModifiedData = ModifiedData.Concat(mData2).ToArray();

                // CLIENT VERSION (This field is no longer used for negotiation and is hardcoded to the 1.2 version)
                pos += 2;

                // CLIENT RANDOM (32 bytes constant)
                pos += 32;

                // SESSION ID
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Session ID error.";
                    return;
                }

                // Session ID Length (1 byte length)
                len = data[pos];
                pos += 1 + len;

                // CIPHER SUITES
                if (pos + 2 > dataLength)
                {
                    ReasonPhrase = "Cipher Suit error.";
                    return;
                }

                // Cipher Suits Length (2 bytes length)
                len = (data[pos] << 8) + data[pos + 1];
                pos += 2 + len;

                // COMPRESSION METHODS (TLS 1.3 no longer allows compression, so this field is always a single entry.
                // 01 - 1 bytes of compression methods. 00 - assigned value for "null" compression.)
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Compression Method error.";
                    return;
                }

                // Compression Methods Length (1 byte length)
                len = data[pos];
                pos += 1 + len;

                if (pos == dataLength && tls_version_major == 3 && tls_version_minor == 0)
                {
                    ReasonPhrase = "Received SSL 3.0 handshake without extensions.";
                    return;
                }

                // EXTENSIONS
                if (pos + 2 > dataLength)
                {
                    ReasonPhrase = "Extensions error.";
                    return;
                }

                // Extensions Length (2 bytes length)
                len = (data[pos] << 8) + data[pos + 1];
                pos += 2;

                //== Create Modified Data
                byte[] mData3 = new byte[pos - mPos];
                Buffer.BlockCopy(data, mPos, mData3, 0, mData3.Length);
                mPos += mData3.Length;
                if (Padding > 0)
                {
                    int padding = len + Padding + 4;
                    mData3[mData3.Length - 2] = (byte)(padding >> 8);
                    mData3[mData3.Length - 1] = (byte)padding;

                    int len2 = (mData3[mData3.Length - 2] << 8) + mData3[mData3.Length - 1];
                    Debug.WriteLine(len2);
                }
                ModifiedData = ModifiedData.Concat(mData3).ToArray();

                if (pos + len > dataLength)
                {
                    ReasonPhrase = "Wrong Data.";
                    return;
                }

                byte[] extensionsData = new byte[len];
                Buffer.BlockCopy(data, pos, extensionsData, 0, extensionsData.Length);

                ParseExtensions(extensionsData, pos);
            }
        }

        private void ParseExtensions(byte[] data, int pos0)
        {
            if (data.Length <= 0) return;

            HasTlsExtensions = true;
            AllExtensions.Data = data;
            AllExtensions.Length = data.Length;
            AllExtensions.StartIndex = pos0;

            int pos = 0;
            int len;

            // Put SNI Extension Last 1
            byte[] sniExtensionLast = Array.Empty<byte>();
            bool hasSniLast = false;

            // Parse each 4 bytes for the extension header (to avoid index out of range)
            while (pos + 4 <= data.Length)
            {
                len = 2; // Add Extension Type
                len += 2; // Add Extension Length (2 bytes length)

                // Add SNI Extension Data
                len += (data[pos + 2] << 8) + data[pos + 3];

                byte[] extData = new byte[len];
                Buffer.BlockCopy(data, pos, extData, 0, len);

                //if (data[pos] == 0x00 && data[pos + 1] == 0x15) // Extension: Padding
                if (data[pos] == 0x00 && data[pos + 1] == 0x00) // Extension: SNI
                {
                    ParseSniExtension(extData, pos0 + pos);

                    hasSniLast = true;
                    sniExtensionLast = extData;

                    if (!PutSniLast)
                    {
                        //== Create Modified Data
                        if (Padding > 0)
                            ModifiedData = ModifiedData.Concat(GetPadding()).ToArray(); // Add Padding before SNI extension
                        ModifiedData = ModifiedData.Concat(extData).ToArray(); // Add SNI Extension
                    }
                }
                else
                {
                    //== Create Modified Data for other extensions
                    ModifiedData = ModifiedData.Concat(extData).ToArray();
                }
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x0b) // Extension: EC Point Formats
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x0a) // Extension: Supported Groups
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x23) // Extension: Session Ticket
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x16) // Extension: Encrypt-Then-MAC
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x17) // Extension: Extended Master Secret
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x0d) // Extension: Signature Algorithms
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x2b) // Extension: Supported Versions
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x2d) // Extension: PSK Key Exchange Modes
                //else if (data[pos] == 0x00 && data[pos + 1] == 0x33) // Extension: Key Share

                // Advance to the next extension
                pos += len;
            }

            // Put SNI Extension Last 2
            if (PutSniLast && hasSniLast)
            {
                //== Create Modified Data
                if (Padding > 0)
                    ModifiedData = ModifiedData.Concat(GetPadding()).ToArray(); // Add Padding before SNI extension
                ModifiedData = ModifiedData.Concat(sniExtensionLast).ToArray(); // Add SNI Extension
            }

            if (SniList.Any())
            {
                HasSni = true;
                ReasonPhrase = "Successfully read SNI.";
            }
            else
            {
                HasSni = false;
                ReasonPhrase = "Wrong Data.";
                SniList.Clear();
            }
        }

        private void ParseSniExtension(byte[] data, int pos0)
        {
            // EXTENSION SERVER NAME
            if (data.Length <= 0) return;

            // Google SNI Extension
            //byte[] google = new byte[23];
            //google[0] = 0;
            //google[1] = 0;
            //google[2] = 0;
            //google[3] = 19;
            //google[4] = 0;
            //google[5] = 17;
            //google[6] = 0;
            //google[7] = 0;
            //google[8] = 14;
            //byte[] googleSNI = Encoding.UTF8.GetBytes("www.google.com");
            //Buffer.BlockCopy(googleSNI, 0, google, 9, googleSNI.Length);
            //data = google;

            HasSniExtension = true;
            SniExtension sniExtension = new();
            sniExtension.Data = data;
            sniExtension.Length = data.Length;
            sniExtension.StartIndex = pos0;
            SniExtensionList.Add(sniExtension);

            int pos = 0;

            // Check if it's a server name extension
            if (data[pos] == 0x00 && data[pos + 1] == 0x00)
            {
                pos += 2; // skip server name list length (00 00)
                int len;

                while (pos + 1 < data.Length)
                {
                    // SNI Extension Data Length (2 bytes length)
                    len = (data[pos] << 8) + data[pos + 1];
                    pos += 2;

                    // First and Only List Entry Length (2 bytes length)
                    len = (data[pos] << 8) + data[pos + 1];
                    pos += 2; // skip extension header

                    // List Entry Type - 0x00 is DNS Hostname (1 byte)
                    if (data[pos] == 0x00)
                    {
                        pos += 1; // Skip List Entry Type

                        // Hostname Length (2 bytes length)
                        len = (data[pos] << 8) + data[pos + 1];
                        pos += 2; // Skip Hostname Length

                        if (pos + len > data.Length) break;

                        if (len > 0)
                        {
                            byte[] outData = new byte[len];
                            Buffer.BlockCopy(data, pos, outData, 0, len);

                            string serverName = Encoding.UTF8.GetString(outData);
                            Debug.WriteLine("----------Server Name: " + serverName + ", Length: " + len + ", Whole Data Length: " + Data.Length);

                            SNI sni = new();
                            sni.Data = outData;
                            sni.Length = len;
                            sni.ServerName = serverName;
                            sni.StartIndex = pos0 + pos;

                            // Add SNI to List
                            SniList.Add(sni);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("SniReader: Unknown server name extension name type.");
                    }

                    pos += len; // Skip Hostname
                }
            }

        }

        // Padding is a TLS Extension (We add Padding before SNI extension)
        private byte[] GetPadding()
        {
            int padding = Padding + 4;

            byte[] paddingData = new byte[padding];

            // Padding Extension Type (2 bytes)
            paddingData[0] = 0x00;
            paddingData[1] = 0x15;

            // Padding Extension Data Length (2 bytes length)
            paddingData[2] = (byte)(Padding >> 8);
            paddingData[3] = (byte)Padding;

            // Add Padding Data which is bunch of zeros
            for (int n = 4; n < padding; n++)
            {
                paddingData[n] = 0x00;
            }

            return paddingData;
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
}
