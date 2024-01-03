using Android;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace SdcMaui
{
    public partial class MainPage
    {
        public void Log(string message, Color? color = null, Editor? editor = null)
        {
            color ??= Colors.DodgerBlue;
            editor ??= log;
            Dispatcher.DispatchIt(() =>
            {
                editor.TextColor = color;
                editor.Text = message;
            });
        }

        public async Task<bool> IsPermitionsGranted()
        {
            if (!OperatingSystem.IsAndroid()) return true;

            PermissionStatus networkState = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
            if (networkState != PermissionStatus.Granted)
            {
                networkState = await Permissions.RequestAsync<Permissions.NetworkState>();
                if (networkState != PermissionStatus.Granted)
                {
                    Log("Network State Permition is Required.", Colors.IndianRed);
                    return false;
                }
            }

            if ((int)Build.VERSION.SdkInt >= 33)
            {
                bool isNotificationGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                if (!isNotificationGranted)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                    isNotificationGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                    if (!isNotificationGranted)
                    {
                        Log("Notification Permition is Required.", Colors.IndianRed);
                        return false;
                    }
                }

                if (Platform.CurrentActivity != null)
                {
                    Permission notification = ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.PostNotifications);
                    if (notification != Permission.Granted)
                    {
                        ActivityCompat.RequestPermissions(Platform.CurrentActivity, new[] { Manifest.Permission.PostNotifications }, 1);
                        notification = ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.PostNotifications);
                        if (notification != Permission.Granted)
                        {
                            Log("Notification Permition is Required.", Colors.IndianRed);
                            return false;
                        }
                    }
                }
            }

            //PermissionStatus StorageRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            //if (StorageRead != PermissionStatus.Granted)
            //{
            //    StorageRead = await Permissions.RequestAsync<Permissions.StorageRead>();
            //    if (StorageRead != PermissionStatus.Granted)
            //    {
            //        Log("Storage Read Permition is Required.", Colors.IndianRed);
            //        return false;
            //    }
            //}

            return true;
        }

    }
}
