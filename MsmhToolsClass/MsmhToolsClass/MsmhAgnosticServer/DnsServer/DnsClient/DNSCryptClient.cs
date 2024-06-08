using MsmhToolsClass.ExternLibs;
using MsmhToolsClass.ProxifiedClients;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

// https://github.com/DNSCrypt/dnscrypt-protocol/blob/master/DNSCRYPT-V2-PROTOCOL.txt
// https://github.com/DNSCrypt/dnscrypt-protocol/blob/master/draft-denis-dprive-dnscrypt.md
// https://dnscrypt.github.io/dnscrypt-protocol/draft-denis-dprive-dnscrypt.html
// https://github.com/DNSCrypt/dnscrypt-protocol/blob/master/ANONYMIZED-DNSCRYPT.txt
public class DNSCryptClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private DnsReader Reader { get; set; } = new();
    private int TimeoutMS { get; set; } = 5;
    private CancellationToken CT { get; set; }
    private string? ProxyScheme { get; set; }
    private string? ProxyUser { get; set; }
    private string? ProxyPass { get; set; }

    private TextRecord.TXTCertificate Certificate { get; set; } = new();

    public DNSCryptClient(byte[] queryBuffer, DnsReader reader, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null, CancellationToken cT = default)
    {
        QueryBuffer = queryBuffer;
        Reader = reader;
        TimeoutMS = timeoutMS;
        ProxyScheme = proxyScheme;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
        CT = cT;
    }

    private async Task<bool> InitializeAsync(DnsEnums.DnsProtocol protocol)
    {
        bool result = false;
        Task task = Task.Run(async () =>
        {
            try
            {
                DnsMessage initializeQuery = DnsMessage.CreateQuery(protocol, Reader.StampReader.ProviderName, DnsEnums.RRType.TEXT, DnsEnums.CLASS.IN);
                DnsMessage.TryWrite(initializeQuery, out byte[] initializeQueryBuffer);

                IPEndPoint ep = new(Reader.StampReader.IP, Reader.Port);

                bool upStreamProxyApplied = false;
                TcpClient? tcpClient = null;
                Socket? initSocket = null;

                try
                {
                    if (protocol == DnsEnums.DnsProtocol.UDP)
                        initSocket = new(ep.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    else if (protocol == DnsEnums.DnsProtocol.TCP)
                    {
                        tcpClient = new();
                        initSocket = tcpClient.Client;

                        // Support Upstream Proxy
                        ProxifiedTcpClient proxifiedTcpClient = new(ProxyScheme, ProxyUser, ProxyPass);
                        var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(ep);
                        if (upstream.isSuccess && upstream.proxifiedTcpClient != null)
                        {
                            tcpClient = upstream.proxifiedTcpClient;
                            initSocket = tcpClient.Client;
                            upStreamProxyApplied = true;
                        }
                    }

                    if (initSocket != null)
                    {
                        initSocket.SendTimeout = TimeoutMS;
                        initSocket.ReceiveTimeout = TimeoutMS;

                        if (!upStreamProxyApplied) await initSocket.ConnectAsync(ep, CT).ConfigureAwait(false);
                        await initSocket.SendAsync(initializeQueryBuffer, SocketFlags.None, CT).ConfigureAwait(false);

                        byte[] initBuffer = new byte[MsmhAgnosticServer.MaxDataSize];
                        int receivedLength = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            receivedLength = await initSocket.ReceiveAsync(initBuffer, SocketFlags.None, CT).ConfigureAwait(false);
                            if (receivedLength > 0) break;
                        }

                        //Debug.WriteLine("========= DnsCrypt ReceivedLength => " + receivedLength);
                        initBuffer = initBuffer[..receivedLength];

                        bool isCertValid = false;
                        uint serial = 0;

                        DnsMessage verifyInitDM = DnsMessage.Read(initBuffer, protocol);
                        foreach (IResourceRecord answer in verifyInitDM.Answers.AnswerRecords)
                        {
                            if (answer is not TextRecord textRecord) continue;
                            //Debug.WriteLine("Number Of Certs: " + textRecord.TXTCertificates.Count);
                            foreach (var cert in textRecord.TXTCertificates)
                            {
                                if (cert.Serial > serial)
                                {
                                    serial = cert.Serial;
                                    Certificate = cert;
                                }

                                byte[] serverSignature = Convert.FromHexString(cert.Signature);
                                TextRecord.TXTCertificate.TryWrite(cert, out byte[] certBuffer);
                                byte[] afterServerSignature = certBuffer[72..];
                                byte[] providerPublicKey = Convert.FromHexString(Reader.StampReader.PublicKey);

                                int verify = LibSodium.crypto_sign_verify_detached(serverSignature, afterServerSignature, afterServerSignature.Length, providerPublicKey);
                                if (verify == 0) isCertValid = true;
                            }
                        }

                        result = isCertValid;
                    }
                }
                catch (Exception) { }

                initSocket?.Shutdown(SocketShutdown.Both);
                initSocket?.Close();
                _ = Task.Run(() => initSocket?.Dispose());
                _ = Task.Run(() => tcpClient?.Dispose());
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        Task task = Task.Run(async () =>
        {
            try
            {
                bool isInitialized = await InitializeAsync(DnsEnums.DnsProtocol.UDP);
                if (!isInitialized) isInitialized = await InitializeAsync(DnsEnums.DnsProtocol.TCP);

                //Debug.WriteLine("Is Certificate Valid: " + isInitialized);
                if (isInitialized)
                {
                    byte[] clientNonce = ByteArrayTool.GenerateRandom(12);
                    byte[] clientNoncePad = new byte[12];
                    byte[] paddedClientNonce = clientNonce.Concat(clientNoncePad).ToArray();

                    byte[] queryPad = GenerateQueryPad(QueryBuffer.Length);
                    byte[] paddedQuery = QueryBuffer.Concat(queryPad).ToArray();

                    byte[] certPublicKey = Convert.FromHexString(Certificate.PublicKey);

                    byte[] clientSecretKey = ByteArrayTool.GenerateRandom(32);
                    
                    bool isCreateSharedKeySuccess = CreateSharedKey(Certificate, certPublicKey, clientSecretKey, out byte[] sharedKeyBuffer);
                    //Debug.WriteLine("Is Create SharedKey Success: " + isCreateSharedKeySuccess);
                    //Debug.WriteLine("SharedKey: " + Convert.ToHexString(sharedKeyBuffer).ToLower());
                    if (isCreateSharedKeySuccess)
                    {
                        bool isEncryptedQuerySuccess = Encrypt(Certificate, ref paddedQuery, ref paddedClientNonce, sharedKeyBuffer, out byte[] encryptedQuery);
                        //Debug.WriteLine("Is Encrypted Query Success: " + isEncryptedQuerySuccess);
                        //Debug.WriteLine("Encrypted Query: " + Convert.ToHexString(encryptedQuery).ToLower());
                        if (isEncryptedQuerySuccess)
                        {
                            bool isCreateClientPublicKeySuccess = CreateClientPublicKey(clientSecretKey, out byte[] clientPublicKey);
                            //Debug.WriteLine("Is Create Client PublicKey Success: " + isCreateClientPublicKeySuccess);
                            //Debug.WriteLine("Client PublicKey: " + Convert.ToHexString(clientPublicKey).ToLower());
                            if (isCreateClientPublicKeySuccess)
                            {
                                //Debug.WriteLine("Certificate ClientMagic: " + Certificate.ClientMagic);
                                List<byte> dnsCryptQueryList = new();
                                dnsCryptQueryList.AddRange(Convert.FromHexString(Certificate.ClientMagic));
                                dnsCryptQueryList.AddRange(clientPublicKey);
                                dnsCryptQueryList.AddRange(clientNonce);
                                dnsCryptQueryList.AddRange(encryptedQuery);
                                byte[] dnsCryptQuery = dnsCryptQueryList.ToArray();

                                IPEndPoint ep = new(Reader.StampReader.IP, Reader.Port);

                                if (Reader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt)
                                {
                                    byte[] anonMagic = new byte[10] { 0xff , 0xff , 0xff , 0xff , 0xff , 0xff , 0xff , 0xff , 0x00 , 0x00 };

                                    IPAddress serverIp = Reader.StampReader.IP;
                                    if (NetworkTool.IsIPv4(serverIp)) serverIp = serverIp.MapToIPv6();

                                    byte[] serverPort = new byte[2];
                                    BinaryPrimitives.WriteUInt16BigEndian(serverPort, (ushort)Reader.Port);

                                    List<byte> anonDnsCryptQueryList = new();
                                    anonDnsCryptQueryList.AddRange(anonMagic);
                                    anonDnsCryptQueryList.AddRange(serverIp.GetAddressBytes());
                                    anonDnsCryptQueryList.AddRange(serverPort);
                                    anonDnsCryptQueryList.AddRange(dnsCryptQuery);

                                    dnsCryptQuery = anonDnsCryptQueryList.ToArray();

                                    ep = new(Reader.DNSCryptRelayIP, Reader.DNSCryptRelayPort);
                                }

                                TcpClient tcpClient = new();
                                tcpClient.SendTimeout = TimeoutMS;
                                tcpClient.ReceiveTimeout = TimeoutMS;
                                tcpClient.Client.NoDelay = true;

                                // Support Upstream Proxy
                                ProxifiedTcpClient proxifiedTcpClient = new(ProxyScheme, ProxyUser, ProxyPass);
                                var upstream = await proxifiedTcpClient.TryGetConnectedProxifiedTcpClient(ep);
                                if (upstream.isSuccess && upstream.proxifiedTcpClient != null) tcpClient = upstream.proxifiedTcpClient;

                                try
                                {
                                    if (!upstream.isSuccess)
                                        await tcpClient.Client.ConnectAsync(ep, CT).ConfigureAwait(false);
                                    //Debug.WriteLine("Is TCP Socket Connected: " + tcpClient.Client.Connected);

                                    byte[] prefix = new byte[2];
                                    BinaryPrimitives.WriteUInt16BigEndian(prefix, (ushort)dnsCryptQuery.Length);
                                    byte[] queryPacketToSend = prefix.Concat(dnsCryptQuery).ToArray();

                                    await tcpClient.Client.SendAsync(queryPacketToSend, SocketFlags.None).ConfigureAwait(false);
                                    //Debug.WriteLine("Is Query Sent: True");

                                    int lengthP = await tcpClient.Client.ReceiveAsync(prefix, SocketFlags.None).ConfigureAwait(false);
                                    //Debug.WriteLine("=== Received Prefix Length: " + lengthP);
                                    ushort size = BinaryPrimitives.ReadUInt16BigEndian(prefix);

                                    if (size > 0)
                                    {
                                        byte[] answerPacket = new byte[size];
                                        int lengthR = await tcpClient.Client.ReceiveAsync(answerPacket, SocketFlags.None).ConfigureAwait(false);
                                        if (answerPacket[0] == 0)
                                            lengthR = await tcpClient.Client.ReceiveAsync(answerPacket, SocketFlags.None).ConfigureAwait(false);
                                        if (answerPacket[0] == 0)
                                            lengthR = await tcpClient.Client.ReceiveAsync(answerPacket, SocketFlags.None).ConfigureAwait(false);
                                        if (answerPacket[0] == 0)
                                            lengthR = await tcpClient.Client.ReceiveAsync(answerPacket, SocketFlags.None).ConfigureAwait(false);

                                        //Debug.WriteLine("=== Received Answer Length: " + lengthR);

                                        string clientMagic = Encoding.UTF8.GetString(answerPacket[..8]);
                                        //Debug.WriteLine("Received Client Magic: " + clientMagic);

                                        string constClientMagic = "r6fnvWj8";
                                        if (clientMagic != constClientMagic)
                                            Debug.WriteLine("Invalid DNSCrypt Client Magic Received.");

                                        if (!clientNonce.SequenceEqual(answerPacket[8..20]))
                                            Debug.WriteLine("Invalid DNSCrypt Client Nonce Received.");

                                        byte[] serverNonce = answerPacket[20..32];
                                        byte[] nonce = clientNonce.Concat(serverNonce).ToArray();

                                        byte[] encryptedAnswer = answerPacket[32..];

                                        bool isDecryptedAnswerSuccess = Decrypt(Certificate, ref encryptedAnswer, ref nonce, sharedKeyBuffer, out byte[] decryptedAnswer);
                                        //Debug.WriteLine("Is Decrypted Answer Success: " + isDecryptedAnswerSuccess);
                                        if (isDecryptedAnswerSuccess)
                                            result = decryptedAnswer;
                                    }
                                }
                                catch (Exception) { }

                                tcpClient.Client.Shutdown(SocketShutdown.Both);
                                tcpClient.Client.Close();
                                _ = Task.Run(() => tcpClient.Dispose());
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }

    private static bool CreateClientPublicKey(byte[] clientSecretKey, out byte[] clientPublicKey)
    {
        try
        {
            byte[] publicKey = new byte[clientSecretKey.Length];
            int result = LibSodium.crypto_scalarmult_base(publicKey, clientSecretKey);
            if (result == 0)
            {
                clientPublicKey = publicKey;
                return true;
            }
            clientPublicKey = Array.Empty<byte>();
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptDns CreateClientPublicKey: " + ex.Message);
            clientPublicKey = Array.Empty<byte>();
            return false;
        }
    }

    private static bool CreateSharedKey(TextRecord.TXTCertificate certificate, byte[] certPublicKey, byte[] secretKey, out byte[] sharedKey)
    {
        try
        {
            byte[] sharedKeyText = new byte[32];
            if (certificate.Version == TextRecord.ESVersion.X25519_XSalsa20Poly1305)
            {
                int result = LibSodium.crypto_box_beforenm(sharedKeyText, certPublicKey, secretKey);
                if (result == 0)
                {
                    sharedKey = sharedKeyText;
                    return true;
                }
            }
            else if (certificate.Version == TextRecord.ESVersion.X25519_XChacha20Poly1305)
            {
                int result = LibSodium.crypto_box_curve25519xchacha20poly1305_beforenm(sharedKeyText, certPublicKey, secretKey);
                if (result == 0)
                {
                    sharedKey = sharedKeyText;
                    return true;
                }
            }
            sharedKey = Array.Empty<byte>();
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptDns CreateSharedKey: " + ex.Message);
            sharedKey = Array.Empty<byte>();
            return false;
        }
    }

    private static bool Encrypt(TextRecord.TXTCertificate certificate, ref byte[] paddedQuery, ref byte[] clientNonce, byte[] sharedKey, out byte[] encrypted)
    {
        try
        {
            byte[] encryptedText = new byte[paddedQuery.Length + 16];
            if (certificate.Version == TextRecord.ESVersion.X25519_XSalsa20Poly1305)
            {
                int result = LibSodium.crypto_box_easy_afternm(encryptedText, paddedQuery, paddedQuery.Length, clientNonce, sharedKey);
                if (result == 0)
                {
                    encrypted = encryptedText;
                    return true;
                }
            }
            else if (certificate.Version == TextRecord.ESVersion.X25519_XChacha20Poly1305)
            {
                int result = LibSodium.crypto_box_curve25519xchacha20poly1305_easy_afternm(encryptedText, paddedQuery, paddedQuery.Length, clientNonce, sharedKey);
                if (result == 0)
                {
                    encrypted = encryptedText;
                    return true;
                }
            }
            encrypted = Array.Empty<byte>();
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptDns Encrypt: " + ex.Message);
            encrypted = Array.Empty<byte>();
            return false;
        }
    }

    private static bool Decrypt(TextRecord.TXTCertificate certificate, ref byte[] encryptedAnswer, ref byte[] serverNonce, byte[] sharedKey, out byte[] decrypted)
    {
        try
        {
            byte[] decryptedText = new byte[encryptedAnswer.Length - 16];
            if (certificate.Version == TextRecord.ESVersion.X25519_XSalsa20Poly1305)
            {
                int result = LibSodium.crypto_box_open_easy_afternm(decryptedText, encryptedAnswer, encryptedAnswer.Length, serverNonce, sharedKey);
                if (result == 0)
                {
                    decrypted = decryptedText;
                    return true;
                }
            }
            else if (certificate.Version == TextRecord.ESVersion.X25519_XChacha20Poly1305)
            {
                int result = LibSodium.crypto_box_curve25519xchacha20poly1305_open_easy_afternm(decryptedText, encryptedAnswer, encryptedAnswer.Length, serverNonce, sharedKey);
                if (result == 0)
                {
                    decrypted = decryptedText;
                    return true;
                }
            }
            decrypted = Array.Empty<byte>();
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptDns Decrypt: " + ex.Message);
            decrypted = Array.Empty<byte>();
            return false;
        }
    }

    private static byte[] GenerateQueryPad(int queryLength)
    {
        byte[] pad = Array.Empty<byte>();

        try
        {
            if (queryLength < 256) pad = new byte[256 - queryLength];

            if (queryLength > 256)
            {
                int paddingLength = 256 + 64;
                while (paddingLength < queryLength)
                {
                    paddingLength += 64;
                }
                pad = new byte[paddingLength - queryLength];
            }

            pad[0] = 0x80;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNSCryptDns GenerateQueryPad: " + ex.Message);
        }

        return pad;
    }
}
