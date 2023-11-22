using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MsmhToolsClass.MsmhProxyServer;

public partial class MsmhProxyServer
{
    /// <summary>
    ///     Tries to get root domain from a given hostname
    ///     Adapted from below answer
    ///     https://stackoverflow.com/questions/16473838/get-domain-name-of-a-url-in-c-sharp-net
    /// </summary>
    /// <param name="hostname"></param>
    /// <returns></returns>
    public static string GetWildCardDomainName(string hostname, bool disableWildCardCertificates = false)
    {
        // only for subdomains we need wild card
        // example www.google.com or gstatic.google.com
        // but NOT for google.com or IP address

        if (IPAddress.TryParse(hostname, out _))
        {
            return hostname;
        }

        if (disableWildCardCertificates)
        {
            return hostname;
        }

        var split = hostname.Split('.');

        if (split.Length > 2)
        {
            // issue #769
            // do not create wildcard if second level domain like: pay.vn.ua
            if (split[0] != "www" && split[1].Length <= 3)
            {
                return hostname;
            }

            int idx = hostname.IndexOf('.');

            // issue #352
            if (hostname[..idx].Contains('-'))
            {
                return hostname;
            }

            string rootDomain = hostname[(idx + 1)..];
            return "*." + rootDomain;
        }

        // return as it is
        return hostname;
    }


    public static string ReadMessage(SslStream sslStream)
    {
        // Read the  message sent by the client.
        // The client signals the end of the message using the
        // "<EOF>" marker.
        byte[] buffer = new byte[2048];
        StringBuilder messageData = new StringBuilder();
        int bytes = -1;
        do
        {
            // Read the client's test message.
            bytes = sslStream.Read(buffer, 0, buffer.Length);

            // Use Decoder class to convert from bytes to UTF8
            // in case a character spans two buffers.
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            messageData.Append(chars);
            // Check for EOF or an empty message.
            if (messageData.ToString().IndexOf("<EOF>") != -1)
            {
                break;
            }
        } while (bytes != 0);

        return messageData.ToString();
    }
    public static void DisplaySecurityLevel(SslStream stream)
    {
        Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
        Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
        Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
        Console.WriteLine("Protocol: {0}", stream.SslProtocol);
    }
    public static void DisplaySecurityServices(SslStream stream)
    {
        Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
        Console.WriteLine("IsSigned: {0}", stream.IsSigned);
        Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        Console.WriteLine("Is mutually authenticated: {0}", stream.IsMutuallyAuthenticated);
    }
    public static void DisplayStreamProperties(SslStream stream)
    {
        Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
        Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
    }
    public static void DisplayCertificateInformation(SslStream stream)
    {
        Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

        X509Certificate localCertificate = stream.LocalCertificate;
        if (stream.LocalCertificate != null)
        {
            Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                localCertificate.Subject,
                localCertificate.GetEffectiveDateString(),
                localCertificate.GetExpirationDateString());
        }
        else
        {
            Console.WriteLine("Local certificate is null.");
        }
        // Display the properties of the client's certificate.
        X509Certificate remoteCertificate = stream.RemoteCertificate;
        if (stream.RemoteCertificate != null)
        {
            Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                remoteCertificate.Subject,
                remoteCertificate.GetEffectiveDateString(),
                remoteCertificate.GetExpirationDateString());
        }
        else
        {
            Console.WriteLine("Remote certificate is null.");
        }
    }
    public static void DisplayUsage()
    {
        Console.WriteLine("To start the server specify:");
        Console.WriteLine("serverSync certificateFile.cer");
        Environment.Exit(1);
    }

    // The following method is invoked by the RemoteCertificateValidationDelegate.
    public static bool ValidateServerCertificate(object sender,
                                                 X509Certificate certificate,
                                                 X509Chain chain,
                                                 SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None) return true;

        Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

        // Do not allow this client to communicate with unauthenticated servers.
        return false;
    }
}


public class SslTcpClient
{
    private static Hashtable certificateErrors = new Hashtable();

    public static bool ValidateServerCertificate(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

        return false;
    }
    public static TcpClient? RunClient(string machineName, string serverName)
    {
        TcpClient client = new("127.0.0.1", 8081);
        Console.WriteLine("Client connected.");
        //NetworkStream networkStream = new()
        SslStream sslStream = new(
            client.GetStream(),
            false,
            new RemoteCertificateValidationCallback(ValidateServerCertificate),
            null
            );
        try
        {
            sslStream.AuthenticateAsClient("127.0.0.1");
        }
        catch (AuthenticationException e)
        {
            Console.WriteLine("Exception: {0}", e.Message);
            Console.WriteLine(e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
            }
            Console.WriteLine("Authentication failed - closing the connection.");
            client.Close();
            return null;
        }
        byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.<EOF>");
        sslStream.Write(messsage);
        sslStream.Flush();
        string serverMessage = ReadMessage(sslStream);
        Console.WriteLine("Server says: {0}", serverMessage);
        //client.Close();
        //Console.WriteLine("Client closed.");
        return client;
    }
    public static string ReadMessage(SslStream sslStream)
    {
        byte[] buffer = new byte[2048];
        StringBuilder messageData = new StringBuilder();
        int bytes = -1;
        do
        {
            bytes = sslStream.Read(buffer, 0, buffer.Length);
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            messageData.Append(chars);
            if (messageData.ToString().IndexOf("<EOF>") != -1)
            {
                break;
            }
        } while (bytes != 0);

        return messageData.ToString();
    }
    public static int Main(string[] args)
    {
        string serverCertificateName = "127.0.0.1";
        string machineName = "127.0.0.1";
        SslTcpClient.RunClient(machineName, serverCertificateName);
        return 0;
    }
}