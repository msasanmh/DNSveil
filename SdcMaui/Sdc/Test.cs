using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdcMaui
{
    //[Android.Runtime.Register("android/net/VpnService", DoNotGenerateAcw = true)]
    public partial class MainPage
    {
        public void Android2()
        {

            //string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#if ANDROID
            var msg = Android.Content.Context.VpnManagementService;


#endif

        }
    }
}
