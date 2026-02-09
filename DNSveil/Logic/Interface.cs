namespace DNSveil.Logic;

public class Interface
{
    public List<string> BindDataSource = new();

    public enum InterfaceItem
    {
        None = 0, SystemProxy = 1, TUN = 2
    }

    public struct InterfaceName
    {
        public const string None = "None";
        public const string SystemProxy = "System Proxy";
        public const string TUN = "Tun";
    }

    public Interface()
    {
        BindDataSource.Add(InterfaceName.None);
        BindDataSource.Add(InterfaceName.SystemProxy);
        BindDataSource.Add(InterfaceName.TUN);
    }

    public InterfaceItem GetInterfaceItem(string interfaceName)
    {
        return interfaceName switch
        {
            InterfaceName.SystemProxy => InterfaceItem.SystemProxy,
            InterfaceName.TUN => InterfaceItem.TUN,
            _ => InterfaceItem.None
        };
    }
}