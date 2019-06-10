using System;

namespace hidden_console
{

    class Program
    {

        static void Main(string[] args)
        {
            IntPtr hWndConsole = HiddenWinConsole.GetConsoleWindow();
            HiddenWinConsole.ShowWindow(hWndConsole, 0);
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
