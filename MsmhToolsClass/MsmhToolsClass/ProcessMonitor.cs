using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.Diagnostics;

namespace MsmhToolsClass;

public sealed class ProcessMonitor : IDisposable
{
    private TraceEventSession? EtwSession;
    private bool AllNetPIDs = true;
    private List<int> NetPidList = new();
    private readonly NetStatistics PNetStatistics = new();
    private readonly List<ConnectedDevices> ConnectedDevicesList = new();
    private readonly System.Timers.Timer ClearTimer = new(30000);
    private bool BypassLocal = false;
    private bool Stop = false;

    public class NetStatistics
    {
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes => BytesSent + BytesReceived;
        public long UploadSpeed { get; set; }
        public long MaxUploadSpeed { get; set; }
        public long DownloadSpeed { get; set; }
        public long MaxDownloadSpeed { get; set; }
    }

    public class ConnectedDevices
    {
        public string DeviceIP { get; set; } = string.Empty;
        public int ProcessID { get; set; }
        public string ProcessName { get; set; } = string.Empty;
    }

    public ProcessMonitor()
    {
        Stop = false;
        ClearTimer.Elapsed -= ClearTimer_Elapsed;
        ClearTimer.Elapsed += ClearTimer_Elapsed;
    }

    private void ClearTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (ConnectedDevicesList)
        {
            ConnectedDevicesList.Clear();
        }
    }

    /// <summary>
    /// Measure One PID
    /// </summary>
    /// <param name="pid">PID</param>
    public void LimitNetPID(int pid)
    {
        AllNetPIDs = false;
        lock (NetPidList)
        {
            NetPidList.Clear();
            NetPidList.Add(pid);
        }
    }

    /// <summary>
    /// Measure A List Of PIDs
    /// </summary>
    /// <param name="pids">PIDs</param>
    public void LimitNetPID(List<int> pids)
    {
        AllNetPIDs = false;
        lock (NetPidList)
        {
            NetPidList.Clear();
            NetPidList = new(pids);
        }
    }

    /// <summary>
    /// Measure Whole System
    /// </summary>
    public void LimitNetPID()
    {
        AllNetPIDs = true;
        lock (NetPidList)
        {
            NetPidList.Clear();
        }
    }

    public void Start(bool bypassLocal)
    {
        BypassLocal = bypassLocal;
        if (!ClearTimer.Enabled) ClearTimer.Start();
        CalcSpeed();

        Task.Run(() =>
        {
            try
            {
                ResetNetCounters();

                EtwSession = new TraceEventSession("MyKernelAndClrEventsSession");
                EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                // Upload TCP
                EtwSession.Source.Kernel.TcpIpSend -= Kernel_TcpIpSend;
                EtwSession.Source.Kernel.TcpIpSend += Kernel_TcpIpSend;

                // Upload UDP
                EtwSession.Source.Kernel.UdpIpSend -= Kernel_UdpIpSend;
                EtwSession.Source.Kernel.UdpIpSend += Kernel_UdpIpSend;

                // Download TCP
                EtwSession.Source.Kernel.TcpIpRecv -= Kernel_TcpIpRecv;
                EtwSession.Source.Kernel.TcpIpRecv += Kernel_TcpIpRecv;

                // Download UDP
                EtwSession.Source.Kernel.UdpIpRecv -= Kernel_UdpIpRecv;
                EtwSession.Source.Kernel.UdpIpRecv += Kernel_UdpIpRecv;

                EtwSession.StopOnDispose = false;
                EtwSession.Source.Process();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProcessMonitor Start: " + ex.Message);
                ResetNetCounters();
            }
        });
    }

    private void Kernel_TcpIpSend(Microsoft.Diagnostics.Tracing.Parsers.Kernel.TcpIpSendTraceData obj)
    {
        if (obj == null) return;
        if (!AllNetPIDs && !IsNetPidMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            PNetStatistics.BytesSent += obj.size;
        });
        task.Wait();
    }

    private void Kernel_UdpIpSend(Microsoft.Diagnostics.Tracing.Parsers.Kernel.UdpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllNetPIDs && !IsNetPidMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            PNetStatistics.BytesSent += obj.size;
        });
        task.Wait();
    }

    private void Kernel_TcpIpRecv(Microsoft.Diagnostics.Tracing.Parsers.Kernel.TcpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllNetPIDs && !IsNetPidMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            PNetStatistics.BytesReceived += obj.size;
        });
        task.Wait();
    }

    private void Kernel_UdpIpRecv(Microsoft.Diagnostics.Tracing.Parsers.Kernel.UdpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllNetPIDs && !IsNetPidMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            PNetStatistics.BytesReceived += obj.size;
        });
        task.Wait();
    }

    private void CalcSpeed()
    {
        Task.Run(async () =>
        {
            while (!Stop)
            {
                long us = 0, ds = 0;

                try
                {
                    long u1 = PNetStatistics.BytesSent;
                    long d1 = PNetStatistics.BytesReceived;
                    await Task.Delay(500);
                    long u2 = PNetStatistics.BytesSent;
                    long d2 = PNetStatistics.BytesReceived;
                    await Task.Delay(500);
                    long u3 = PNetStatistics.BytesSent;
                    long d3 = PNetStatistics.BytesReceived;

                    long us1 = (u2 - u1) * 2;
                    long us2 = (u3 - u2) * 2;
                    long us3 = u3 - u1;

                    long ds1 = (d2 - d1) * 2;
                    long ds2 = (d3 - d2) * 2;
                    long ds3 = d3 - d1;

                    long[] usArray = new long[] { us1, us2, us3 };
                    us = Convert.ToInt64(usArray.Average());

                    long[] dsArray = new long[] { ds1, ds2, ds3 };
                    ds = Convert.ToInt64(dsArray.Average());
                }
                catch (Exception) { }

                PNetStatistics.UploadSpeed = us;
                if (us > PNetStatistics.MaxUploadSpeed) PNetStatistics.MaxUploadSpeed = us;
                PNetStatistics.DownloadSpeed = ds;
                if (ds > PNetStatistics.MaxDownloadSpeed) PNetStatistics.MaxDownloadSpeed = ds;
            }
        });
    }

    private bool IsNetPidMatch(int pid)
    {
        for (int n = 0; n < NetPidList.Count; n++)
            if (NetPidList[n] == pid) return true;
        return false;
    }

    private void AddNewConnectedDevice(string ipStr, int pid, string processName)
    {
        if (!NetworkTool.IsLocalIP(ipStr)) return;

        ConnectedDevices cd = new()
        {
            DeviceIP = ipStr,
            ProcessID = pid,
            ProcessName = processName
        };

        if (!IsConnectedDeviceExist(cd))
        {
            lock (ConnectedDevicesList)
            {
                ConnectedDevicesList.Add(cd);
            }
        }
    }

    private bool IsConnectedDeviceExist(ConnectedDevices cd)
    {
        lock (ConnectedDevicesList)
        {
            for (int n = 0; n < ConnectedDevicesList.Count; n++)
                if (ConnectedDevicesList[n].DeviceIP.Equals(cd.DeviceIP) && ConnectedDevicesList[n].ProcessID == cd.ProcessID)
                    return true;
            return false;
        }
    }

    // ========== Public Methods

    public NetStatistics GetNetStatistics()
    {
        return PNetStatistics;
    }

    public List<ConnectedDevices> GetConnectedDevices()
    {
        List<ConnectedDevices> connectedDevices = new(ConnectedDevicesList);
        return connectedDevices;
    }

    public void ResetNetCounters()
    {
        PNetStatistics.BytesSent = 0;
        PNetStatistics.BytesReceived = 0;
        PNetStatistics.UploadSpeed = 0;
        PNetStatistics.MaxUploadSpeed = 0;
        PNetStatistics.DownloadSpeed = 0;
        PNetStatistics.MaxDownloadSpeed = 0;

        lock (ConnectedDevicesList)
        {
            ConnectedDevicesList.Clear();
        }
    }

    public void Dispose()
    {
        try
        {
            if (ClearTimer.Enabled) ClearTimer.Stop();
            Stop = true;
            EtwSession?.Source.StopProcessing();
            EtwSession?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ProcessMonitor Dispose: " + ex.Message);
        }
    }
}