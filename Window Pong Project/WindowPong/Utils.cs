using System;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using WindowPong.Native;
/*
 * Written by Tomasz Galka 2019 (Tommy.galk@gmail.com)
 * Class containing several utility methods.
 */
namespace WindowPong.Utility
{
    public class Utils
    {
        private static string[] excludedProcesses = {"svchost", "system", "csrss", "conhost", "ekrn", "dllhost", "dwm", "lsass"};
        private static Process recentProcess;
        //Method written according to the formula on this website (https://www.rapidtables.com/convert/color/hsv-to-rgb.html)
        public static Color HSVtoRGB(int h, int s, int v)
        {
            float r = 0, g = 0, b = 0;
            h = Constrain(h, 0, 360);
            s = Constrain(s, 0, 100);
            v = Constrain(v, 0, 100);
            s /= 100;
            v /= 100;
            float c = v * s;
            float hDeg = h / 60f;
            float x = c * (1f - Math.Abs((hDeg % 2f) - 1f));
            float m = v - c;
            if (hDeg >= 0 && hDeg < 1)
            {
                r = c;
                g = x;
            }
            else if (hDeg >= 1 && hDeg < 2)
            {
                r = x;
                g = c;
            }
            else if (hDeg >= 2 && hDeg < 3)
            {
                g = c;
                b = x;
            }
            else if (hDeg >= 3 && hDeg < 4)
            {
                g = x;
                b = c;
            }
            else if (hDeg >= 4 && hDeg < 5)
            {
                r = x;
                b = c;
            }
            else
            {
                r = c;
                b = x;
            }
            r = (r + m) * 255f;
            g = (g + m) * 255f;
            b = (b + m) * 255f;
            return Color.FromArgb(255, (int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        public static Process ScanForNewProcess(bool excludeSystemProcesses) {
            Process[] processes = Process.GetProcesses();

            Process newestProcess = GetNewestProcess(processes);
            if (recentProcess != null && recentProcess.ProcessName == newestProcess.ProcessName)
            {
                return null;
            } else {
                bool r = excludeSystemProcesses ? IsExcludedProcess(newestProcess) : false;
                if (r) return null;
                recentProcess = newestProcess;
                return newestProcess;
            }
        }

        private static Process GetNewestProcess(Process[] processes) {
            Process n = processes[processes.Length - 1];
            foreach (Process p in processes) {
                try
                {
                    if (DateTime.Compare(p.StartTime, n.StartTime) >= 0)
                    {
                        n = p;
                    }
                }
                catch (System.ComponentModel.Win32Exception E)
                {
                    Console.WriteLine("PROC: " + p.ProcessName + " E: " + E.Message);
                    continue;
                }
                catch (System.InvalidOperationException) {
                    continue;
                }
            }
            return n;
        }

        private static bool IsExcludedProcess(Process proc) {
            foreach (string n in excludedProcesses) {
                if (proc.ProcessName.ToLower().Equals(n))
                    return true;
            }
            return false;
        }

        public static string RepeatCharacter(char c, int count)
        {
            if (count <= 1) return c + "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string RepeatString(string s, int count)
        {
            if (count <= 1) return s;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static string GetTabs(int count)
        {
            return RepeatString(GetTab(), count);
        }

        public static string GetTab() //I don't remember why I needed this tbh, like \t exists?!
        {
            return RepeatCharacter(' ', 8);
        }

        public static int Constrain(int value, int min, int max)
        {
            return (value < min) ? min : ((value > max) ? max : value);
        }

        public static float Constrain(float value, float min, float max)
        {
            return (value < min) ? min : ((value > max) ? max : value);
        }

        public static float Map(float val, float rs, float re, float r1s, float r1e) {
            return r1s + (r1e - r1s) * ((val - rs) / (re - rs));
        }

        public static bool OffScreen()
        {
            return false;
        }

        /**********Only used for testing window sizes**********/
        private static Vector size;
        private static Vector oldSize;

        public static void TestWindowSize()
        {
            while (true)
            {
                Thread.Sleep(10);
                Process[] processes = Process.GetProcessesByName("notepad");

                if (processes.Length > 0)
                {
                    NativeUtils.Rect bounds = new NativeUtils.Rect();
                    NativeUtils.GetWindowRect(processes[0].MainWindowHandle, ref bounds);
                    size = new Vector(bounds.GetWidth(), bounds.GetHeight());
                }

                if (!size.CompareTo(oldSize))
                {
                    oldSize = size;
                    Console.WriteLine("Width: " + size.x + " | Height: " + size.y);
                }
            }
        }
        /**********Only used for testing window sizes**********/
    }
}
