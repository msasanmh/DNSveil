using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MsmhToolsClass;

// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapter
public class NetworkInterfaces
{
    // AdapterType // String
    /// <summary>
    /// Network medium in use.
    /// </summary>
    public string AdapterType { get; private set; } = string.Empty;
    
    // AdapterTypeID // uint16
    /// <summary>
    /// Network medium in use. Returns the same information as the AdapterType property,
    /// except that the information is in the form of an integer.
    /// </summary>
    public ushort AdapterTypeID { get; private set; }
    public string AdapterTypeIDMessage => GetAdapterType(AdapterTypeID);
    private static string GetAdapterType(ushort n)
    {
        return n switch
        {
            0 => "Ethernet 802.3",
            1 => "Token Ring 802.5",
            2 => "Fiber Distributed Data Interface (FDDI)",
            3 => "Wide Area Network (WAN)",
            4 => "LocalTalk",
            5 => "Ethernet using DIX header format",
            6 => "ARCNET",
            7 => "ARCNET (878.2)",
            8 => "ATM",
            9 => "Wireless",
            10 => "Infrared Wireless",
            11 => "Bpc",
            12 => "CoWan",
            13 => "1394",
            _ => "Unknown"
        };
    }

    // AutoSense // Boolean
    // If True, the network adapter can automatically determine the speed of the attached or network media.
    // This property has not been implemented yet. It returns a NULL value by default.

    // Availability // uint16
    /// <summary>
    /// Availability and status of the device.
    /// </summary>
    public ushort Availability { get; private set; }
    public string AvailabilityMessage => GetAvailability(Availability);
    private static string GetAvailability(ushort n)
    {
        return n switch
        {
            1 => "Other",
            2 => "Unknown",
            3 => "Running/Full Power",
            4 => "Warning",
            5 => "In Test",
            6 => "Not Applicable",
            7 => "Power Off",
            8 => "Off Line",
            9 => "Off Duty",
            10 => "Degraded",
            11 => "Not Installed",
            12 => "Install Error",
            13 => "Power Save - Unknown",
            14 => "Power Save - Low Power Mode",
            15 => "Power Save - Standby",
            16 => "Power Cycle",
            17 => "Power Save - Warning",
            18 => "Paused",
            19 => "Not Ready",
            20 => "Not Configured",
            21 => "Quiesced",
            _ => "Unknown"
        };
    }

    // Caption // String
    /// <summary>
    /// Caption
    /// </summary>
    public string Caption { get; private set; } = string.Empty;

    // ConfigManagerErrorCode // uint32
    /// <summary>
    /// Windows Configuration Manager error code.
    /// </summary>
    public uint ConfigManagerErrorCode { get; private set; }
    public string ConfigManagerErrorCodeMessage => GetConfigManagerErrorCode(ConfigManagerErrorCode);
    private static string GetConfigManagerErrorCode(uint n)
    {
        return n switch
        {
            0 => "This device is working properly.",
            1 => "This device is not configured correctly.",
            2 => "Windows cannot load the driver for this device.",
            3 => "The driver for this device might be corrupted, or your system may be running low on memory or other resources.",
            4 => "This device is not working properly. One of its drivers or your registry might be corrupted.",
            5 => "The driver for this device needs a resource that Windows cannot manage.",
            6 => "The boot configuration for this device conflicts with other devices.",
            7 => "Cannot filter.",
            8 => "The driver loader for the device is missing.",
            9 => "This device is not working properly because the controlling firmware is reporting the resources for the device incorrectly.",
            10 => "This device cannot start.",
            11 => "This device failed.",
            12 => "This device cannot find enough free resources that it can use.",
            13 => "Windows cannot verify this device's resources.",
            14 => "This device cannot work properly until you restart your computer.",
            15 => "This device is not working properly because there is probably a re-enumeration problem.",
            16 => "Windows cannot identify all the resources this device uses.",
            17 => "This device is asking for an unknown resource type.",
            18 => "Reinstall the drivers for this device.",
            19 => "Failure using the VxD loader.",
            20 => "Your registry might be corrupted.",
            21 => "System failure: Try changing the driver for this device. If that does not work, see your hardware documentation. Windows is removing this device.",
            22 => "This device is disabled.",
            23 => "System failure: Try changing the driver for this device. If that doesn't work, see your hardware documentation.",
            24 => "This device is not present, is not working properly, or does not have all its drivers installed.",
            25 => "Windows is still setting up this device.",
            26 => "Windows is still setting up this device.",
            27 => "This device does not have valid log configuration.",
            28 => "The drivers for this device are not installed.",
            29 => "This device is disabled because the firmware of the device did not give it the required resources.",
            30 => "This device is using an Interrupt Request (IRQ) resource that another device is using.",
            31 => "This device is not working properly because Windows cannot load the drivers required for this device.",
            _ => "Unknown"
        };
    }

    // ConfigManagerUserConfig // Boolean
    /// <summary>
    /// If True, the device is using a user-defined configuration.
    /// </summary>
    public bool ConfigManagerUserConfig { get; private set; }

    // CreationClassName // String
    /// <summary>
    /// Name of the first concrete class to appear in the inheritance chain used in the creation of an instance.
    /// When used with the other key properties of the class, the property allows all instances of this class and its subclasses to be uniquely identified.
    /// </summary>
    public string CreationClassName { get; private set; } = string.Empty;

    // Description // String
    /// <summary>
    /// Description of the object.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    // DeviceID // String
    /// <summary>
    /// Unique identifier of the network adapter from other devices on the system.
    /// </summary>
    public string DeviceID { get; private set; } = string.Empty;

    // ErrorCleared // Boolean
    /// <summary>
    /// If True, the error reported in LastErrorCode is now cleared.
    /// </summary>
    public bool ErrorCleared { get; private set; }

    // ErrorDescription // String
    /// <summary>
    /// More information about the error recorded in LastErrorCode, and information about any corrective actions that may be taken.
    /// </summary>
    public string ErrorDescription { get; private set; } = string.Empty;

    // GUID // String
    /// <summary>
    /// Globally unique identifier for the connection.
    /// </summary>
    public string GUID { get; private set; } = string.Empty;

    // Index // uint32
    /// <summary>
    /// Index number of the network adapter, stored in the system registry.
    /// </summary>
    public uint Index { get; private set; }

    // InstallDate // DateTime
    /// <summary>
    /// Date and time the object was installed. This property does not need a value to indicate that the object is installed.
    /// </summary>
    public DateTime InstallDate { get; private set; }

    // Installed // Boolean
    /// <summary>
    /// If True, the network adapter is installed in the system.
    /// </summary>
    public bool Installed { get; private set; }

    // InterfaceIndex // uint32
    /// <summary>
    /// Index value that uniquely identifies the local network interface.
    /// The value in this property is the same as the value in the InterfaceIndex property
    /// in the instance of Win32_IP4RouteTable that represents the network interface in the route table.
    /// </summary>
    public uint InterfaceIndex { get; private set; }

    // LastErrorCode // uint32
    /// <summary>
    /// Last error code reported by the logical device.
    /// </summary>
    public uint LastErrorCode { get; private set; }

    // MACAddress // String
    /// <summary>
    /// Media access control address for this network adapter.
    /// </summary>
    public string MACAddress { get; private set; } = string.Empty;

    // Manufacturer // String
    /// <summary>
    /// Name of the network adapter's manufacturer.
    /// </summary>
    public string Manufacturer { get; private set; } = string.Empty;

    // MaxNumberControlled // uint32
    /// <summary>
    /// Maximum number of directly addressable ports supported by this network adapter.
    /// A value of 0 (zero) should be used if the number is unknown.
    /// </summary>
    public uint MaxNumberControlled { get; private set; } = 0;

    // MaxSpeed // uint64
    // Maximum speed, in bits per second, for the network adapter.
    // This property has not been implemented yet. It returns a NULL value by default.

    // Name // String
    /// <summary>
    /// Label by which the object is known. When subclassed, the property can be overridden to be a key property.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    // NetConnectionID // String
    // Access type: Read/write
    /// <summary>
    /// Name of the network connection as it appears in the Network Connections Control Panel program.
    /// </summary>
    public string NetConnectionID { get; private set; } = string.Empty;

    // NetConnectionStatus // uint16
    /// <summary>
    /// State of the network adapter connection to the network. (0 - 65535)
    /// </summary>
    public ushort NetConnectionStatus { get; private set; }
    public string NetConnectionStatusMessage => GetNetConnectionStatus(NetConnectionStatus);
    private static string GetNetConnectionStatus(ushort n)
    {
        return n switch
        {
            0 => "Disconnected",
            1 => "Connecting",
            2 => "Connected",
            3 => "Disconnecting",
            4 => "Hardware Not Present",
            5 => "Hardware Disabled",
            6 => "Hardware Malfunction",
            7 => "Media Disconnected",
            8 => "Authenticating",
            9 => "Authentication Succeeded",
            10 => "Authentication Failed",
            11 => "Invalid Address",
            12 => "Credentials Required",
            _ => "Other"
        };
    }

    // NetEnabled // Boolean
    /// <summary>
    /// Indicates whether the adapter is enabled or not. If True, the adapter is enabled.
    /// You can enable or disable the NIC by using the Enable and Disable methods.
    /// </summary>
    public bool NetEnabled { get; private set; }

    // NetworkAddresses // string array
    // Array of network addresses for an adapter.
    // This property has not been implemented yet. It returns a NULL value by default.

    // PermanentAddress // String
    /// <summary>
    /// Network address hard-coded into an adapter.
    /// This hard-coded address may be changed by firmware upgrade or software configuration.
    /// If so, this field should be updated when the change is made.
    /// The property should be left blank if no hard-coded address exists for the network adapter.
    /// </summary>
    public string PermanentAddress { get; private set; } = string.Empty;

    // PhysicalAdapter // Boolean
    /// <summary>
    /// Indicates whether the adapter is a physical or a logical adapter. If True, the adapter is physical.
    /// </summary>
    public bool PhysicalAdapter { get; private set; }

    // PNPDeviceID // String
    /// <summary>
    /// Windows Plug and Play device identifier of the logical device.
    /// </summary>
    public string PNPDeviceID { get; private set; } = string.Empty;

    // PowerManagementCapabilities // uint16 array
    /// <summary>
    /// Array of the specific power-related capabilities of a logical device.
    /// </summary>
    public ushort[] PowerManagementCapabilities { get; private set; } = Array.Empty<ushort>();
    public string[] PowerManagementCapabilitiesMessage
    {
        get
        {
            string[] strings = new string[PowerManagementCapabilities.Length];
            for (int n = 0; n < PowerManagementCapabilities.Length; n++)
                strings[n] = GetPowerManagementCapabilities(PowerManagementCapabilities[n]);
            return strings;
        }
    }
    private static string GetPowerManagementCapabilities(ushort n)
    {
        return n switch
        {
            0 => "Unknown",
            1 => "Not Supported",
            2 => "Disabled",
            3 => "Enabled",
            4 => "Power Saving Modes Entered Automatically",
            5 => "Power State Settable",
            6 => "Power Cycling Supported",
            7 => "Timed Power On Supported",
            _ => "Unknown"
        };
    }

    // PowerManagementSupported // Boolean
    /// <summary>
    /// If True, the device can be power-managed (can be put into suspend mode, and so on).
    /// The property does not indicate that power management features are currently enabled,
    /// only that the logical device is capable of power management.
    /// </summary>
    public bool PowerManagementSupported { get; private set; }

    // ProductName // String
    /// <summary>
    /// Product name of the network adapter.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    // ServiceName // String
    /// <summary>
    /// Service name of the network adapter. This name is usually shorter than the full product name.
    /// </summary>
    public string ServiceName { get; private set; } = string.Empty;

    // Speed // uint64
    /// <summary>
    /// Estimate of the current bandwidth in bits per second.
    /// For endpoints which vary in bandwidth or for those where no accurate estimation can be made,
    /// this property should contain the nominal bandwidth.
    /// </summary>
    public ulong Speed { get; private set; }

    // Status // String
    /// <summary>
    /// Current status of the object.
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    // StatusInfo // uint16
    /// <summary>
    /// State of the logical device.
    /// If this property does not apply to the logical device, the value 5 (Not Applicable) should be used.
    /// </summary>
    public ushort StatusInfo { get; private set; }
    public string StatusInfoMessage => GetStatusInfo(StatusInfo);
    private static string GetStatusInfo(ushort n)
    {
        return n switch
        {
            1 => "Other",
            2 => "Unknown",
            3 => "Enabled",
            4 => "Disabled",
            5 => "Not Applicable",
            _ => "Unknown"
        };
    }

    // SystemCreationClassName // String
    /// <summary>
    /// Value of the scoping computer's CreationClassName property.
    /// </summary>
    public string SystemCreationClassName { get; private set; } = string.Empty;

    // SystemName // String
    /// <summary>
    /// Name of the scoping system.
    /// </summary>
    public string SystemName { get; private set; } = string.Empty;

    // TimeOfLastReset // DateTime
    /// <summary>
    /// Date and time the network adapter was last reset.
    /// </summary>
    public DateTime TimeOfLastReset { get; private set; }

    // Adding My Own Stuff
    /// <summary>
    /// Get NetworkInterface if NIC is Enabled.
    /// </summary>
    public NetworkInterface? NIC { get; private set; }

    public List<string> DnsAddresses { get; private set; } = new();

    /// <summary>
    /// Get NIC Information
    /// </summary>
    /// <param name="nicName">NIC Name (NetConnectionID)</param>
    public NetworkInterfaces(string nicName)
    {
        if (!OperatingSystem.IsWindows()) return;
        ObjectQuery? query = new("SELECT * FROM Win32_NetworkAdapter");

        using ManagementObjectSearcher searcher = new(query);
        ManagementObjectCollection queryCollection = searcher.Get();

        foreach (ManagementBaseObject m in queryCollection)
        {
            object nnObj = m[nameof(NetConnectionID)];
            if (nnObj == null) continue;
            string nn = nnObj.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(nn) && nn.Trim().Equals(nicName.Trim()))
            {
                object adapterTypeObj = m[nameof(AdapterType)];
                if (adapterTypeObj != null)
                    AdapterType = adapterTypeObj.ToString() ?? string.Empty;

                try
                {
                    AdapterTypeID = Convert.ToUInt16(m[nameof(AdapterTypeID)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(AdapterTypeID)}: {ex.Message}");
                }

                try
                {
                    Availability = Convert.ToUInt16(m[nameof(Availability)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(Availability)}: {ex.Message}");
                }

                object captionObj = m[nameof(Caption)];
                if (captionObj != null)
                    Caption = captionObj.ToString() ?? string.Empty;

                try
                {
                    ConfigManagerErrorCode = Convert.ToUInt32(m[nameof(ConfigManagerErrorCode)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ConfigManagerErrorCode)}: {ex.Message}");
                }

                try
                {
                    ConfigManagerUserConfig = Convert.ToBoolean(m[nameof(ConfigManagerUserConfig)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ConfigManagerUserConfig)}: {ex.Message}");
                }

                object creationClassNameObj = m[nameof(CreationClassName)];
                if (creationClassNameObj != null)
                    CreationClassName = creationClassNameObj.ToString() ?? string.Empty;

                object descriptionObj = m[nameof(Description)];
                if (descriptionObj != null)
                    Description = descriptionObj.ToString() ?? string.Empty;

                object deviceIDObj = m[nameof(DeviceID)];
                if (deviceIDObj != null)
                    DeviceID = deviceIDObj.ToString() ?? string.Empty;

                try
                {
                    ErrorCleared = Convert.ToBoolean(m[nameof(ErrorCleared)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ErrorCleared)}: {ex.Message}");
                }

                object errorDescriptionObj = m[nameof(ErrorDescription)];
                if (errorDescriptionObj != null)
                    ErrorDescription = errorDescriptionObj.ToString() ?? string.Empty;

                object guidObj = m[nameof(GUID)];
                if (guidObj != null)
                    GUID = guidObj.ToString() ?? string.Empty;

                try
                {
                    Index = Convert.ToUInt32(m[nameof(Index)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(Index)}: {ex.Message}");
                }

                try
                {
                    object obj = m[nameof(InstallDate)];
                    if (obj != null)
                    {
                        string dt = obj.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(dt))
                        {
                            if (dt.Contains('.')) dt = dt.Split('.')[0];
                            bool ok = DateTime.TryParseExact(dt, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);
                            if (ok)
                                InstallDate = dateTime;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(InstallDate)}: {ex.Message}");
                }

                try
                {
                    Installed = Convert.ToBoolean(m[nameof(Installed)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(Installed)}: {ex.Message}");
                }

                try
                {
                    InterfaceIndex = Convert.ToUInt32(m[nameof(InterfaceIndex)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(InterfaceIndex)}: {ex.Message}");
                }

                try
                {
                    LastErrorCode = Convert.ToUInt32(m[nameof(LastErrorCode)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(LastErrorCode)}: {ex.Message}");
                }

                object macAddressObj = m[nameof(MACAddress)];
                if (macAddressObj != null)
                    MACAddress = macAddressObj.ToString() ?? string.Empty;

                object manufacturerObj = m[nameof(Manufacturer)];
                if (manufacturerObj != null)
                    Manufacturer = manufacturerObj.ToString() ?? string.Empty;

                try
                {
                    MaxNumberControlled = Convert.ToUInt32(m[nameof(MaxNumberControlled)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(MaxNumberControlled)}: {ex.Message}");
                }

                object nameObj = m[nameof(Name)];
                if (nameObj != null)
                    Name = nameObj.ToString() ?? string.Empty;

                object netConnectionIDObj = m[nameof(NetConnectionID)];
                if (netConnectionIDObj != null)
                    NetConnectionID = netConnectionIDObj.ToString() ?? string.Empty;

                try
                {
                    NetConnectionStatus = Convert.ToUInt16(m[nameof(NetConnectionStatus)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(NetConnectionStatus)}: {ex.Message}");
                }

                try
                {
                    NetEnabled = Convert.ToBoolean(m[nameof(NetEnabled)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(NetEnabled)}: {ex.Message}");
                }

                object permanentAddressObj = m[nameof(PermanentAddress)];
                if (permanentAddressObj != null)
                    PermanentAddress = permanentAddressObj.ToString() ?? string.Empty;

                try
                {
                    PhysicalAdapter = Convert.ToBoolean(m[nameof(PhysicalAdapter)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(PhysicalAdapter)}: {ex.Message}");
                }

                object pnpDeviceIDObj = m[nameof(PNPDeviceID)];
                if (pnpDeviceIDObj != null)
                    PNPDeviceID = pnpDeviceIDObj.ToString() ?? string.Empty;

                object pmcObj = m[nameof(PowerManagementCapabilities)];
                if (pmcObj != null)
                {
                    string pmc = pmcObj.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(pmc))
                    {
                        char[] nums = pmc.ToArray();
                        ushort[] ushorts = new ushort[nums.Length];
                        for (int n = 0; n < nums.Length; n++)
                        {
                            try
                            {
                                ushorts[n] = Convert.ToUInt16(nums[n]);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"{nameof(PowerManagementCapabilities)}: {ex.Message}");
                            }
                        }
                        PowerManagementCapabilities = ushorts;
                    }
                }
                
                try
                {
                    PowerManagementSupported = Convert.ToBoolean(m[nameof(PowerManagementSupported)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(PowerManagementSupported)}: {ex.Message}");
                }

                object productNameObj = m[nameof(ProductName)];
                if (productNameObj != null)
                    ProductName = productNameObj.ToString() ?? string.Empty;

                object serviceNameObj = m[nameof(ServiceName)];
                if (serviceNameObj != null)
                    ServiceName = serviceNameObj.ToString() ?? string.Empty;

                try
                {
                    Speed = Convert.ToUInt64(m[nameof(Speed)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(Speed)}: {ex.Message}");
                }

                object statusObj = m[nameof(Status)];
                if (statusObj != null)
                    Status = statusObj.ToString() ?? string.Empty;

                try
                {
                    StatusInfo = Convert.ToUInt16(m[nameof(StatusInfo)]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(StatusInfo)}: {ex.Message}");
                }

                object systemCreationClassNameObj = m[nameof(SystemCreationClassName)];
                if (systemCreationClassNameObj != null)
                    SystemCreationClassName = systemCreationClassNameObj.ToString() ?? string.Empty;

                object systemNameObj = m[nameof(SystemName)];
                if (systemNameObj != null)
                    SystemName = systemNameObj.ToString() ?? string.Empty;

                try
                {
                    object obj = m[nameof(TimeOfLastReset)];
                    if (obj != null)
                    {
                        string dt = obj.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(dt))
                        {
                            if (dt.Contains('.')) dt = dt.Split('.')[0];
                            bool ok = DateTime.TryParseExact(dt, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);
                            if (ok)
                                TimeOfLastReset = dateTime;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(TimeOfLastReset)}: {ex.Message}");
                }

                // Get NetworkInterface Format
                NIC = NetworkTool.GetNICByName(nicName);

                // Get DNS Addresses
                if (NIC != null)
                {
                    IPAddressCollection dnss = NIC.GetIPProperties().DnsAddresses;
                    for (int n = 0; n < dnss.Count; n++)
                        DnsAddresses.Add(dnss[n].ToString());
                }

                break;
            }
        }
    }

    /// <summary>
    /// Get All Network Interfaces
    /// </summary>
    /// <returns>A List of NIC Names (NetConnectionID)</returns>
    public static List<string> GetAllNetworkInterfaces(bool upAndRunning)
    {
        List<string> nicNames = new();
        if (!OperatingSystem.IsWindows()) return nicNames;
        ObjectQuery? query = new("SELECT * FROM Win32_NetworkAdapter");

        using ManagementObjectSearcher searcher = new(query);
        ManagementObjectCollection queryCollection = searcher.Get();

        foreach (ManagementBaseObject m in queryCollection)
        {
            object nnObj = m[nameof(NetConnectionID)];
            if (nnObj == null) continue;
            string nn = nnObj.ToString() ?? string.Empty;
            nn = nn.Trim(); // NetConnectionStatus
            if (!string.IsNullOrEmpty(nn))
            {
                ushort up = 0;
                try { up = Convert.ToUInt16(m[nameof(NetConnectionStatus)]); } catch (Exception) { }

                if (upAndRunning)
                {
                    if (up == 2) // Connected
                        nicNames.Add(nn);
                }
                else
                    nicNames.Add(nn);
            }
        }

        return nicNames;
    }


    private static void GetAllNetworkInterfaces22()
    {
        
        if (!OperatingSystem.IsWindows()) return;
        ObjectQuery? query = new("SELECT * FROM Win32_NetworkAdapter");

        using ManagementObjectSearcher searcher = new(query);
        ManagementObjectCollection queryCollection = searcher.Get();

        foreach (ManagementBaseObject m in queryCollection)
        {
            string result = string.Empty;
            result += $"AdapterType: {m[nameof(AdapterType)]}{Environment.NewLine}";
            result += $"AdapterTypeID: {m[nameof(AdapterTypeID)]}{Environment.NewLine}";
            result += $"AutoSense: {m["AutoSense"]}{Environment.NewLine}";
            result += $"Availability: {m[nameof(Availability)]}{Environment.NewLine}";
            result += $"Caption: {m[nameof(Caption)]}{Environment.NewLine}";
            result += $"ConfigManagerErrorCode: {m[nameof(ConfigManagerErrorCode)]}{Environment.NewLine}";
            result += $"ConfigManagerUserConfig: {m[nameof(ConfigManagerUserConfig)]}{Environment.NewLine}";
            result += $"CreationClassName: {m[nameof(CreationClassName)]}{Environment.NewLine}";
            result += $"Description: {m[nameof(Description)]}{Environment.NewLine}";
            result += $"DeviceID: {m[nameof(DeviceID)]}{Environment.NewLine}";
            result += $"ErrorCleared: {m[nameof(ErrorCleared)]}{Environment.NewLine}";
            result += $"ErrorDescription: {m[nameof(ErrorDescription)]}{Environment.NewLine}";
            result += $"GUID: {m[nameof(GUID)]}{Environment.NewLine}";
            result += $"Index: {m[nameof(Index)]}{Environment.NewLine}";
            result += $"InstallDate: {m[nameof(InstallDate)]}{Environment.NewLine}";
            result += $"Installed: {m[nameof(Installed)]}{Environment.NewLine}";
            result += $"InterfaceIndex: {m[nameof(InterfaceIndex)]}{Environment.NewLine}";
            result += $"LastErrorCode: {m[nameof(LastErrorCode)]}{Environment.NewLine}";
            result += $"MACAddress: {m[nameof(MACAddress)]}{Environment.NewLine}";
            result += $"Manufacturer: {m[nameof(Manufacturer)]}{Environment.NewLine}";
            result += $"MaxNumberControlled: {m[nameof(MaxNumberControlled)]}{Environment.NewLine}";
            result += $"MaxSpeed: {m["MaxSpeed"]}{Environment.NewLine}";
            result += $"Name: {m[nameof(Name)]}{Environment.NewLine}";
            result += $"NetConnectionID: {m[nameof(NetConnectionID)]}{Environment.NewLine}";
            result += $"NetConnectionStatus: {m[nameof(NetConnectionStatus)]}{Environment.NewLine}";
            result += $"NetEnabled: {m[nameof(NetEnabled)]}{Environment.NewLine}";
            result += $"NetworkAddresses: {m["NetworkAddresses"]}{Environment.NewLine}";
            result += $"PermanentAddress: {m[nameof(PermanentAddress)]}{Environment.NewLine}";
            result += $"PhysicalAdapter: {m[nameof(PhysicalAdapter)]}{Environment.NewLine}";
            result += $"PNPDeviceID: {m[nameof(PNPDeviceID)]}{Environment.NewLine}";
            result += $"PowerManagementCapabilities: {m[nameof(PowerManagementCapabilities)]}{Environment.NewLine}";
            result += $"PowerManagementSupported: {m[nameof(PowerManagementSupported)]}{Environment.NewLine}";
            result += $"ProductName: {m[nameof(ProductName)]}{Environment.NewLine}";
            result += $"ServiceName: {m[nameof(ServiceName)]}{Environment.NewLine}";
            result += $"Speed: {m[nameof(Speed)]}{Environment.NewLine}";
            result += $"Status: {m[nameof(Status)]}{Environment.NewLine}";
            result += $"StatusInfo: {m[nameof(StatusInfo)]}{Environment.NewLine}";
            result += $"SystemCreationClassName: {m[nameof(SystemCreationClassName)]}{Environment.NewLine}";
            result += $"SystemName: {m[nameof(SystemName)]}{Environment.NewLine}";
            result += $"TimeOfLastReset: {m[nameof(TimeOfLastReset)]}{Environment.NewLine}";
            result += "===========================================";

            Debug.WriteLine(result);
        }
    }
}