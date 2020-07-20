using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace fakemic
{
    class Hooky
    {
        ///////////////////////////////////////////////////////////
        //A bunch of DLL Imports to set a low level keyboard hook
        ///////////////////////////////////////////////////////////
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static event EventHandler CDown;
        public static event EventHandler CUp;

        ////////////////////////////////////////////////////////////////
        //Some constants to make handling our hook code easier to read
        ////////////////////////////////////////////////////////////////
        private const int WH_KEYBOARD_LL = 13;                    //Type of Hook - Low Level Keyboard
        private const int WM_KEYDOWN = 0x0100;                    //Value passed on KeyDown
        private const int WM_KEYUP = 0x0101;                      //Value passed on KeyUp
        private static LowLevelKeyboardProc _proc = HookCallback; //The function called when a key is pressed
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool CONTROL_DOWN = false;                 //Bool to use as a flag for control key

        public static void Initialize()
        {
            _hookID = SetHook(_proc);  //Set our hook
            Application.Run();         //Start a standard application method loop
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) //A Key was pressed down
            {
                int vkCode = Marshal.ReadInt32(lParam);           //Get the keycode
                Keys theKey = ((Keys)vkCode);        //Name of the key
                if(theKey == Keys.C )
                {
                    CDown?.Invoke(null, null);
                }
                //UnhookWindowsHookEx(_hookID);
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) //KeyUP
            {
                int vkCode = Marshal.ReadInt32(lParam);        //Get Keycode
                Keys theKey = ((Keys)vkCode);     //Get Key name
                if(theKey == Keys.C )
                {
                    CUp?.Invoke(null, null);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam); //Call the next hook
        }
    }
}