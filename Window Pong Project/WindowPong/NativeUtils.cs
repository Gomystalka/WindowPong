using System;
using System.Runtime.InteropServices;

/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com)
 * Helper Class for several native method imports.
 * Imports methods from user32.dll and kernel32.dll.
 * All methods and structs were written according to the Microsoft Windows API documentation.
 */

namespace WindowPong.Native
{
    public class NativeUtils
    {
        public struct TagMSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public long time;
            public int pt;
            public IntPtr lPrivate;
        }

        public struct Rect {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int GetWidth() {
                return right - left;
            }

            public int GetHeight() {
                return bottom - top;
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PeekMessage(out TagMSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool TranslateMessage([In] ref TagMSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DispatchMessage([In] ref TagMSG lpmsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextA(IntPtr hWnd, out string lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowTextA(IntPtr hWnd, string lpString);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr handle, ref Rect rect);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        public const uint PM_REMOVE = 0x0001; //Remove message from queue
        public const int WS_EX_LAYERED = 0x80000; //Layered window style
        public const int WS_EX_TRANSPARENT = 0x20; //Transparent window style
        public const int SW_SHOW = 5; //Show window
        public const int SW_RESTORE = 9; //Restore window state
    }
}
