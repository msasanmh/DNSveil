using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SDCHttpProxy
{
    internal class ConsoleTools
    {
        //=============================================== Console Quick Editr
        // Save Original on Startup
        // GetConsoleMode(GetConsoleWindow(), ref saveConsoleMode);
        // Restore at Exit
        // SetConsoleMode(GetConsoleWindow(), saveConsoleMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            int ioMode);

        /// <summary>
        /// This flag enables the user to use the mouse to select and edit text. To enable
        /// this option, you must also set the ExtendedFlags flag.
        /// </summary>
        const int QuickEditMode = 64;

        // ExtendedFlags must be combined with
        // InsertMode and QuickEditMode when setting
        /// <summary>
        /// ExtendedFlags must be enabled in order to enable InsertMode or QuickEditMode.
        /// </summary>
        const int ExtendedFlags = 128;

        public static void DisableQuickEdit()
        {
            IntPtr conHandle = GetConsoleWindow();

            if (!GetConsoleMode(conHandle, out int mode))
            {
                // error getting the console mode. Exit.
                return;
            }

            mode &= ~(QuickEditMode | ExtendedFlags);

            if (!SetConsoleMode(conHandle, mode))
            {
                // error setting console mode.
            }
        }

        public static void EnableQuickEdit()
        {
            IntPtr conHandle = GetConsoleWindow();

            if (!GetConsoleMode(conHandle, out int mode))
            {
                // error getting the console mode. Exit.
                return;
            }

            mode |= (QuickEditMode | ExtendedFlags);

            if (!SetConsoleMode(conHandle, mode))
            {
                // error setting console mode.
            }
        }
    }
}
