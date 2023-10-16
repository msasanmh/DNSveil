using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace MsmhToolsClass;

public sealed class ProcessMonitor : IDisposable
{
    private TraceEventSession? EtwSession;
    private List<int> PidList = new();
    private bool AllPIDs { get; set; } = false;
    private readonly Counters MCounters = new();
    private readonly List<ConnectedDevices> ConnectedDevicesList = new();
    private readonly System.Timers.Timer ClearTimer = new(30000);
    private bool BypassLocal = false;
    private bool Stop = false;

    public class ConnectedDevices
    {
        public string DeviceIP { get; set; } = string.Empty;
        public int ProcessID { get; set; }
        public string ProcessName { get; set; } = string.Empty;
    }

    private class Counters
    {
        public long BytesSent;
        public long BytesReceived;
        public long UploadSpeed;
        public long DownloadSpeed;
    }

    public class ProcessStatistics
    {
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes => BytesSent + BytesReceived;
        public long UploadSpeed { get; set; }
        public long DownloadSpeed { get; set; }
        public List<ConnectedDevices> ConnectedDevices { get; set; } = new();
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
    public void SetPID(int pid)
    {
        AllPIDs = false;
        lock (PidList)
        {
            PidList.Clear();
            PidList.Add(pid);
        }
    }

    /// <summary>
    /// Measure A List Of PIDs
    /// </summary>
    /// <param name="pids">PIDs</param>
    public void SetPID(List<int> pids)
    {
        AllPIDs = false;
        lock (PidList)
        {
            PidList.Clear();
            PidList = new(pids);
        }
    }

    /// <summary>
    /// Measure Whole System
    /// </summary>
    public void SetPID()
    {
        AllPIDs = true;
        lock (PidList)
        {
            PidList.Clear();
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
                ResetCounters();

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
            catch
            {
                ResetCounters();
            }
        });
    }

    private void Kernel_TcpIpSend(Microsoft.Diagnostics.Tracing.Parsers.Kernel.TcpIpSendTraceData obj)
    {
        if (obj == null) return;
        if (!AllPIDs && !IsMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            lock (MCounters)
            {
                MCounters.BytesSent += obj.size;
            }
        });
        task.Wait();
    }

    private void Kernel_UdpIpSend(Microsoft.Diagnostics.Tracing.Parsers.Kernel.UdpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllPIDs && !IsMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            lock (MCounters)
            {
                MCounters.BytesSent += obj.size;
            }
        });
        task.Wait();
    }

    private void Kernel_TcpIpRecv(Microsoft.Diagnostics.Tracing.Parsers.Kernel.TcpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllPIDs && !IsMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            lock (MCounters)
            {
                MCounters.BytesReceived += obj.size;
            }
        });
        task.Wait();
    }

    private void Kernel_UdpIpRecv(Microsoft.Diagnostics.Tracing.Parsers.Kernel.UdpIpTraceData obj)
    {
        if (obj == null) return;
        if (!AllPIDs && !IsMatch(obj.ProcessID)) return;

        Task task = Task.Run(() =>
        {
            AddNewConnectedDevice(obj.saddr.ToString(), obj.ProcessID, obj.ProcessName);
            AddNewConnectedDevice(obj.daddr.ToString(), obj.ProcessID, obj.ProcessName);

            if (BypassLocal && NetworkTool.IsLocalIP(obj.daddr.ToString())) return;

            lock (MCounters)
            {
                MCounters.BytesReceived += obj.size;
            }
        });
        task.Wait();
    }

    private void CalcSpeed()
    {
        Task.Run(async () =>
        {
            while (!Stop)
            {
                long u1 = MCounters.BytesSent;
                long d1 = MCounters.BytesReceived;
                await Task.Delay(1000);
                long u2 = MCounters.BytesSent;
                long d2 = MCounters.BytesReceived;

                lock (MCounters)
                {
                    MCounters.UploadSpeed = u2 - u1;
                    MCounters.DownloadSpeed = d2 - d1;
                }
            }
        });
    }

    public ProcessStatistics GetProcessStatistics()
    {
        ProcessStatistics ps = new();
        Task task = Task.Run(() =>
        {
            try
            {
                lock (MCounters)
                {
                    ps.BytesSent = MCounters.BytesSent;
                    ps.BytesReceived = MCounters.BytesReceived;
                    ps.UploadSpeed = MCounters.UploadSpeed;
                    ps.DownloadSpeed = MCounters.DownloadSpeed;
                }

                lock (ConnectedDevicesList)
                {
                    ps.ConnectedDevices = new(ConnectedDevicesList);
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        });
        task.Wait();
        return ps;
    }

    public void ResetCounters()
    {
        lock (MCounters)
        {
            MCounters.BytesSent = 0;
            MCounters.BytesReceived = 0;
            MCounters.UploadSpeed = 0;
            MCounters.DownloadSpeed = 0;
        }
    }

    private bool IsMatch(int pid)
    {
        for (int n = 0; n < PidList.Count; n++)
            if (PidList[n] == pid) return true;
        return false;
    }

    private void AddNewConnectedDevice(string ipStr, int pid, string processName)
    {
        if (!NetworkTool.IsLocalIP(ipStr)) return;

        ConnectedDevices cd = new();
        cd.DeviceIP = ipStr;
        cd.ProcessID = pid;
        cd.ProcessName = processName;

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

    public void Dispose()
    {
        if (ClearTimer.Enabled) ClearTimer.Stop();
        Stop = true;
        EtwSession?.Source.StopProcessing();
        EtwSession?.Dispose();
    }
}