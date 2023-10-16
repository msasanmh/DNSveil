using System;

namespace MsmhToolsClass;

public class OsTool
{
    public static bool IsWin7()
    {
        bool result = false;
        OperatingSystem os = Environment.OSVersion;
        Version vs = os.Version;

        if (os.Platform == PlatformID.Win32NT)
        {
            if (vs.Minor == 1 && vs.Major == 6)
                result = true;
        }

        return result;
    }
}
