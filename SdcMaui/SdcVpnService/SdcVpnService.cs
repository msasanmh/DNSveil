using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using java.lang;
using java.net;
using Java.IO;
using Java.Lang;
using Java.Nio;
using Java.Nio.Channels;
using Java.Util.Concurrent;
using sun.net;
using Exception = Java.Lang.Exception;
//using Process = Java.Lang.Process;
//using Process = Android.OS.Process;

namespace SdcMaui.Vpn;

[Service(Label = "SdcVpnService", Permission = "android.permission.BIND_VPN_SERVICE")]
public class SdcVpnService : VpnService
{
    private const string TAG = "SdcVpnService";
    private const string VPN_ADDRESS = "10.0.0.0"; // Only IPv4 support for now
    private const string VPN_ROUTE = "0.0.0.0"; // Intercept everything
    private const string DNS_ADDRESS = "8.8.8.8";

    private const string BROADCAST_VPN_STATE = "android.vpnservice.VPN_STATE";

    private static ParcelFileDescriptor? vpnInterface = null;

    private PendingIntent? pendingIntent;

    public static Activity ActivityContext { get; set; }
    public static bool IsPermissionGranted { get; set; } = false;

    private const int RequestVpnPermission = 10;
    public static bool IsRunning { get; private set; }
    private static bool Stop { get; set; }

    public static void RequestPermission()
    {
        // Manage VpnPermission
        if (ActivityContext is null) return;
        if (IsPermissionGranted) return;
        Intent? intent = Prepare(ActivityContext);
        if (intent != null)
        {
            ActivityContext.StartActivityForResult(intent, RequestVpnPermission);
        }
        else
        {
            IsPermissionGranted = true;
        }
    }

    public static void StartVPN()
    {
        if (ActivityContext is null) return;

        RequestPermission();

        if (IsPermissionGranted)
        {
            //StartService(new Intent(this, typeof(LocalVPNService)));

            Intent intent = new(ActivityContext, typeof(SdcVpnService));

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                ActivityContext.StartForegroundService(intent);
            }
            else
            {
                ActivityContext.StartService(intent);
            }
        }
    }

    public static void StopVPN()
    {
        Stop = true;
        Cleanup();
    }

    public override void OnCreate()
    {
        base.OnCreate();

        IsRunning = true;
        SetupVPN();

        if (vpnInterface == null)
        {
            Log.Error(TAG, "Vpn Interface is null. Something when wrong.");
            StopSelf();
            return;
        }
    }

    private void SetupVPN()
    {
        if (vpnInterface == null)
        {
            Builder builder = new(this);
            builder.AddAddress(VPN_ADDRESS, 32);
            builder.AddRoute(VPN_ROUTE, 0);
            builder.AddDnsServer(DNS_ADDRESS);
            builder.SetSession(TAG);
            if (pendingIntent != null)
                builder.SetConfigureIntent(pendingIntent);

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
                builder.SetMetered(false);

            //ProxyInfo? proxyInfo = ProxyInfo.BuildDirectProxy("0.0.0.0", MainPage.ProxyPort);
            //if (proxyInfo != null)
            //    builder.SetHttpProxy(proxyInfo);

            string? packageName = ApplicationContext?.PackageName;
            if (!string.IsNullOrEmpty(packageName))
                builder.AddDisallowedApplication(packageName);

            //using (var pm = Android.App.Application.Context.PackageManager)
            //{
            //    var packageList = pm.GetInstalledApplications(Android.Content.PM.PackageInfoFlags.MatchAll);
            //    foreach (var package in packageList)
            //    {
            //        if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(package.PackageName))
            //        {
            //            if (!package.PackageName.Contains(packageName))
            //            {
            //                try
            //                {
            //                    Log.Debug(TAG, package.PackageName);
            //                    builder.AddAllowedApplication(package.PackageName);
            //                }
            //                catch (Exception ex)
            //                {
            //                    System.Diagnostics.Debug.WriteLine(ex.Message);
            //                }
            //            }
            //        }
            //    }
            //}

            vpnInterface = builder.Establish();
        }

        if (vpnInterface == null) return;
        // Get the file descriptor of the VPN interface
        FileDescriptor? vpnFileDescriptor = vpnInterface.FileDescriptor;
        if (vpnFileDescriptor == null) return;

        // Create a new file input stream from the file descriptor
        FileInputStream vpnInput = new(vpnFileDescriptor);
        
        // Get the file channel of the input stream
        FileChannel? vpnInputChannel = vpnInput.Channel;
        if (vpnInputChannel == null) return;

        // Create a new file output stream from the file descriptor
        FileOutputStream vpnOutput = new(vpnFileDescriptor);

        // Get the file channel of the output stream
        FileChannel? vpnOutputChannel = vpnOutput.Channel;
        if (vpnOutputChannel == null) return;

        //DatagramChannel? tunnel = DatagramChannel.Open();
        //if (tunnel == null) return;

        //Protect(tunnel.Socket());

        //InetSocketAddress server = new ("127.0.0.1", MainPage.ProxyPort);

        //tunnel.Connect(server);
        //tunnel.ConfigureBlocking(false);

        Task.Run(async () =>
        {
            Stop = false;

            //byte[] buf = new byte[short.MaxValue];
            //while (true)
            //{
            //    await Task.Delay(10);
            //    if (Stop) break;
            //    if (!vpnFileDescriptor.Valid()) break;

            //    try
            //    {
            //        int read = vpnInput.Read(buf);
            //        while ((read = vpnInput.Read(buf)) > 0)
            //        {
            //            byte[] packetBuffer = buf[..read]; // copy buffer for packet

            //            PacketDotNet.IPPacket? ipPacket = PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Raw, packetBuffer)?.Extract<PacketDotNet.IPPacket>();
            //            if (ipPacket != null)
            //            {
            //                vpnOutput.Write(ipPacket.Bytes);
            //            }
            //        }

            //    }
            //    catch (Exception) { }
            //}



            while (true)
            {
                Task.Delay(10).Wait();
                if (Stop) break;
                if (!vpnFileDescriptor.Valid()) break;

                try
                {
                    ByteBuffer bufferToNetwork = ByteBufferPool.acquire();

                    // Convert ByteBuffer to Byte[]
                    //byte[] arr = new byte[buffer.Slice().Remaining()];
                    //buffer.Get(arr);

                    // Read the data from the VPN interface
                    int length = await vpnInputChannel.ReadAsync(bufferToNetwork);

                    bool dataSent = true;
                    bool dataReceived = true;

                    while (length > 0)
                    {
                        bufferToNetwork.Limit(length);
                        bufferToNetwork.Flip();
                        Packet packet = new(bufferToNetwork);

                        ConcurrentLinkedQueue output = new();

                        if (packet.IsUDP)
                        {
                            Selector? selector = Selector.Open();
                            if (selector == null)
                            {
                                dataSent = false;
                            }
                            else
                            {
                                ConcurrentLinkedQueue input = new();
                                UDPInput udpInput = new(output, selector);
                                udpInput.Run();
                                UDPOutput udpOutput = new(input, selector, this);
                                udpOutput.Run();
                                input.Offer(packet);
                            }
                        }
                        else if (packet.IsTCP)
                        {
                            Selector? selector = Selector.Open();
                            if (selector == null)
                            {
                                dataSent = false;
                            }
                            else
                            {
                                ConcurrentLinkedQueue input = new();
                                TCPInput tcpInput = new(output, selector);
                                tcpInput.Run();
                                TCPOutput tcpOutput = new(input, output, selector, this);
                                tcpOutput.Run();
                                input.Offer(packet);
                            }
                        }
                        else
                        {
                            dataSent = false;
                        }
                        
                        if (!dataSent)
                        {
                            bufferToNetwork.Clear();
                        }
                        else
                        {
                            ByteBuffer? bufferFromNetwork = (ByteBuffer?)output.Poll();
                            if (bufferFromNetwork != null)
                            {
                                bufferFromNetwork.Flip();
                                while (bufferFromNetwork.HasRemaining)
                                {
                                    try
                                    {
                                        await vpnOutputChannel.WriteAsync(bufferFromNetwork);
                                    }
                                    catch (System.Exception)
                                    {
                                        bufferFromNetwork.Clear();
                                    }
                                }
                            }
                            else
                            {
                                dataReceived = false;
                            }
                        }

                        if (!dataSent && !dataReceived)
                            System.Threading.Thread.Sleep(100);

                        // PacketDotNet
                        //PacketDotNet.IPPacket? ipPacket = PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Raw, arr)?.Extract<PacketDotNet.IPPacket>();
                        
                    }


                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }
        });
    }

    private void debugPacket(ByteBuffer packet)
    {
        /*
        for(int i = 0; i < length; ++i)
        {
            byte buffer = packet.get();

            Log.d(TAG, "byte:"+buffer);
        }*/



        int buffer = packet.Get();
        int version;
        int headerlength;
        version = buffer >> 4;
        headerlength = buffer & 0x0F;
        headerlength *= 4;
        System.Diagnostics.Debug.WriteLine("IP Version:" + version);
        System.Diagnostics.Debug.WriteLine("Header Length:" + headerlength);

        string status = "";
        status += "Header Length:" + headerlength;

        buffer = packet.Get();      //DSCP + EN
        buffer = packet.GetChar(buffer);  //Total Length

        System.Diagnostics.Debug.WriteLine("Total Length:" + buffer);

        buffer = packet.GetChar(buffer);  //Identification
        buffer = packet.GetChar(buffer);  //Flags + Fragment Offset
        buffer = packet.Get();      //Time to Live
        buffer = packet.Get();      //Protocol

        System.Diagnostics.Debug.WriteLine("Protocol:" + buffer);

        status += "  Protocol:" + buffer;

        buffer = packet.GetChar(buffer);  //Header checksum

        string sourceIP = "";
        buffer = packet.Get();  //Source IP 1st Octet
        sourceIP += buffer;
        sourceIP += ".";

        buffer = packet.Get();  //Source IP 2nd Octet
        sourceIP += buffer;
        sourceIP += ".";

        buffer = packet.Get();  //Source IP 3rd Octet
        sourceIP += buffer;
        sourceIP += ".";

        buffer = packet.Get();  //Source IP 4th Octet
        sourceIP += buffer;

        System.Diagnostics.Debug.WriteLine("Source IP:" + sourceIP);

        status += "   Source IP:" + sourceIP;

        string destIP = "";
        buffer = packet.Get();  //Destination IP 1st Octet
        destIP += buffer;
        destIP += ".";

        buffer = packet.Get();  //Destination IP 2nd Octet
        destIP += buffer;
        destIP += ".";

        buffer = packet.Get();  //Destination IP 3rd Octet
        destIP += buffer;
        destIP += ".";

        buffer = packet.Get();  //Destination IP 4th Octet
        destIP += buffer;

        System.Diagnostics.Debug.WriteLine("Destination IP:" + destIP);

        status += "   Destination IP:" + destIP;
        /*
        msgObj = mHandler.obtainMessage();
        msgObj.obj = status;
        mHandler.sendMessage(msgObj);
        */

        //Log.d(TAG, "version:"+packet.getInt());
        //Log.d(TAG, "version:"+packet.getInt());
        //Log.d(TAG, "version:"+packet.getInt());

    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        IsRunning = false;
        //executorService?.ShutdownNow();
        Cleanup();
        Log.Info(TAG, "Stopped");
    }

    private static void Cleanup()
    {
        //deviceToNetworkTCPQueue = null;
        //deviceToNetworkUDPQueue = null;
        //networkToDeviceQueue = null;
        //ByteBufferPool.Clear();
        //CloseResources(udpSelector, tcpSelector, vpnInterface);
        vpnInterface?.Close();
    }




}