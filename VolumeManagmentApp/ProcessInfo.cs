using System;
using System.Runtime.InteropServices;

namespace VolumeManagmentApp
{
    class ProcessInfo
    {
        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(int hWnd, out uint processId);
    }
}
