using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using WindowPong.Input;
using WindowPong.Native;

/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com) 
 * Written according to the Microsoft C++ Windows Hook documentation.
 * Class used to create a Low Level Keyboard Hook to capture/intercept key events.
 */

namespace WindowPong.Hooks
{
    public class KeyboardHook {
        //Constants
        public const byte WH_KEYBOARD_LL = 13; //Keyboard Hook ID
        public const int WM_KEYDOWN = 0x0100; //Key Down ID
        public const int WM_KEYUP = 0x0101; //Key Up ID
        public readonly static IntPtr INTERCEPT = (IntPtr)1; //IntPtr used as a return value for intercepting key events.

        //Single press feature
        public bool SinglePressEnabled { get; set; } //Determines whether the Single Key Press event is recorded.
        public bool[] lockedKeys = new bool[256];

        //Hook
        private readonly int hookType; //Type of hook.
        public bool HookHandleSet { get; set; }
        public IntPtr HookHandle { get; set; } //Handle of the hook as returned by the SetWindowsHookEx method.
        public bool Active { get; set; } //State of the hook. False = No keys recorded/intercepted.
        private NativeUtils.LowLevelKeyboardProc Callback { get; set; } //The Keyboard Hook callback method.
        public List<ushort> InterceptedKeys { get; set; } //Keys that will only be registered by this application.
        public IKeyEventListener KeyListener { get; set; } //An interface used to handle key events.

        public KeyboardHook(int hookType) {
            this.hookType = hookType;
        }

        public void RegisterKeyboardHook() {
            if(KeyListener == null) throw new System.ComponentModel.Win32Exception("A KeyEventListener must be assigned before registering a new keyboard hook!");
            Callback = HandleKeyEvent;
            if (Callback == null) throw new System.ComponentModel.Win32Exception("A Callback method must be assigned before registering a new keyboard hook!");
            if (HookHandleSet) throw new System.ComponentModel.Win32Exception("This keyboard hook has already been registered!");
            HookHandleSet = true;
            HookHandle = NativeUtils.SetWindowsHookEx(hookType, Callback, NativeUtils.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            Console.WriteLine("Keyboard Hook registered." + (InterceptedKeys != null && InterceptedKeys.Count > 0 ? " Intercepted keys: " + GetInterceptedKeyString() : "No intercepted keys."));
        }

        public void RegisterKeyboardHook(IKeyEventListener listener)
        {
            KeyListener = listener;
            RegisterKeyboardHook();
        }

        public IntPtr HandleKeyEvent(int nCode, IntPtr wParam, IntPtr lParam) {
            int keyCode = System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
            if (nCode >= 0 && Active && keyCode <= ushort.MaxValue)
            {
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (SinglePressEnabled && !lockedKeys[keyCode]) {
                        lockedKeys[keyCode] = true;
                        KeyListener.OnSingleKeyPressed((ushort)keyCode); //Oh...
                    }
                    KeyListener.OnKeyPressed((ushort)keyCode);
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    if (SinglePressEnabled)
                        lockedKeys[keyCode] = false;
                    KeyListener.OnKeyReleased((ushort)keyCode);
                }
            }
            if (CheckForInterceptedKeys((ushort)keyCode) && Active) return INTERCEPT;
            return NativeUtils.CallNextHookEx(HookHandle, nCode, wParam, lParam);
        }

        private bool CheckForInterceptedKeys(ushort keyCode) {
            if (InterceptedKeys == null) return false;
            foreach (ushort k in InterceptedKeys) {
                if (keyCode == k) return true;
            }
            return false;
        }

        public void Unhook() {
            NativeUtils.UnhookWindowsHookEx(HookHandle);
        }

        public void AddInterceptedKey(ushort keyCode) {
            if (keyCode < 0) return; //Don't continue if the keyCode is negative.
            if (InterceptedKeys == null) InterceptedKeys = new List<ushort>(); //Assign a new List of integers if it is null.
            if (!InterceptedKeys.Contains(keyCode)) 
                InterceptedKeys.Add(keyCode); //Add a new keyCode to the list if it's not in the list already.
        }

        public string GetInterceptedKeyString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (uint k in InterceptedKeys) {
                sb.Append(k + ", ");
            }
            return sb.ToString().Substring(0, sb.Length - 2) + "]";
        }
    }
}
