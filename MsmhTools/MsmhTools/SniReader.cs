using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools
{
    // https://github.com/dlundquist/sniproxy/blob/master/src/tls.c
    public class SniReader
    {
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

        public string ReasonPhrase { get; private set; } = string.Empty;
        public bool HaveTlsExtensions { get; private set; } = false;
        public TlsExtensions AllExtensions { get; private set; } = new();
        public bool HaveSniExtension { get; private set; } = false;
        public List<SniExtension> SniExtensionList { get; private set; } = new();
        public bool HaveSni { get; private set; } = false;
        public List<SNI> SniList { get; private set; } = new();

        private const int TLS_HEADER_LEN = 5;
        private const int TLS_HANDSHAKE_CONTENT_TYPE = 0x16;
        private const int TLS_HANDSHAKE_TYPE_CLIENT_HELLO = 0x01;

        public SniReader(byte[] data)
        {
            int pos = TLS_HEADER_LEN;
            int dataLength = data.Length;

            if (dataLength < TLS_HEADER_LEN)
            {
                ReasonPhrase = "TCP payload is not large enough for a TLS header.";
                return;
            }

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

                // TLS record length
                int len = (data[3] << 8) + data[4] + TLS_HEADER_LEN;
                dataLength = Math.Min(dataLength, len);

                // Check we received entire TLS record length
                if (dataLength < len)
                {
                    ReasonPhrase = "Didn't receive entire TLS record length.";
                    return;
                }

                // Handshake
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Handshake error.";
                    return;
                }

                if (data[pos] != TLS_HANDSHAKE_TYPE_CLIENT_HELLO)
                {
                    ReasonPhrase = "Not a client hello.";
                    return;
                }

                /* Skip past fixed length records:
                    1	Handshake Type
                    3	Length
                    2	Version (again)
                    32	Random
                    to	Session ID Length
                */
                pos += 38;

                // Session ID
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Session ID error.";
                    return;
                }

                len = data[pos];
                pos += 1 + len;

                // Cipher Suits
                if (pos + 2 > dataLength)
                {
                    ReasonPhrase = "Cipher Suit error.";
                    return;
                }

                len = (data[pos] << 8) + data[pos + 1];
                pos += 2 + len;

                // Compression Methods
                if (pos + 1 > dataLength)
                {
                    ReasonPhrase = "Compression Method error.";
                    return;
                }

                len = data[pos];
                pos += 1 + len;

                if (pos == dataLength && tls_version_major == 3 && tls_version_minor == 0)
                {
                    ReasonPhrase = "Received SSL 3.0 handshake without extensions.";
                    return;
                }

                // Extensions
                if (pos + 2 > dataLength)
                {
                    ReasonPhrase = "Extensions error.";
                    return;
                }

                len = (data[pos] << 8) + data[pos + 1];
                pos += 2;

                if (pos + len > dataLength)
                {
                    ReasonPhrase = "Wrong Data.";
                    return;
                }

                // Extensions Length
                int extensionsLength = Convert.ToInt32(data[pos - 1]);

                byte[] newData = new byte[extensionsLength];
                //byte[] newData = new byte[dataLength - pos];
                Buffer.BlockCopy(data, pos, newData, 0, newData.Length);

                ParseExtensions(newData, pos);
            }
        }

        private void ParseExtensions(byte[] data, int pos0)
        {
            if (data.Length <= 0) return;
            //Debug.WriteLine(data.Length + " == " + dataLength);

            HaveTlsExtensions = true;
            AllExtensions.Data = data;
            AllExtensions.Length = data.Length;
            AllExtensions.StartIndex = pos0;

            int pos = 0;
            int len;

            // Parse each 4 bytes for the extension header (to avoid index out of range)
            while (pos + 4 <= data.Length)
            {
                // extension header
                len = 4;
                // Extension Length - header
                len += (data[pos + 2] << 8) + data[pos + 3];
                
                //Debug.WriteLine($"{pos} L: {len} --- {dataLength}");

                // Check if it's a server name extension
                if (data[pos] == 0x00 && data[pos + 1] == 0x00)
                {
                    byte[] extData = new byte[len];
                    Buffer.BlockCopy(data, pos, extData, 0, extData.Length);

                    ParseServerNameExtension(extData, pos0 + pos);
                }

                // Advance to the next extension
                pos += len;
            }

            if (SniList.Any())
            {
                HaveSni = true;
                ReasonPhrase = "Successfully read SNI.";
            }
            else
            {
                HaveSni = false;
                ReasonPhrase = "Wrong Data.";
                SniList.Clear();
            }
        }

        private void ParseServerNameExtension(byte[] data, int pos0)
        {
            if (data.Length <= 0) return;

            // Length of SNI Extension saved here. SNI Extension Length == SNI Length + 9
            //data[5] = 17;

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

            HaveSniExtension = true;
            SniExtension sniExtension = new();
            sniExtension.Data = data;
            sniExtension.Length = data.Length;
            sniExtension.StartIndex = pos0;
            SniExtensionList.Add(sniExtension);

            int pos = 0;
            
            pos += 2; // skip server name list length
            int len;

            while (pos + 3 < data.Length)
            {
                pos += 4; // skip extension header
                len = (data[pos + 1] << 8) + data[pos + 2];

                int newPos = pos + 3;

                if (newPos + len > data.Length) break;

                if (data[pos] == 0x00)
                {
                    int outDataLength = data.Length - newPos;
                    if (outDataLength > 0)
                    {
                        byte[] outData = new byte[outDataLength];
                        Buffer.BlockCopy(data, newPos, outData, 0, outDataLength);

                        string serverName = Encoding.UTF8.GetString(outData);
                        Debug.WriteLine("Server Name: " + serverName + " Length: " + outDataLength + " = " + (Convert.ToInt32(data[5]) - 3));

                        SNI sni = new();
                        sni.Data = outData;
                        sni.Length = outDataLength;
                        sni.ServerName = serverName;
                        sni.StartIndex = pos0 + newPos;

                        // Add SNI to List
                        SniList.Add(sni);
                    }
                }
                else
                {
                    Debug.WriteLine("SniReader: Unknown server name extension name type.");
                }

                pos += 3 + len;
            }
        }

    }
}
