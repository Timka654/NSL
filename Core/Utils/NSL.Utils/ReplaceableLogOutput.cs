using System;
using System.Threading;

namespace NSL.Utils
{
    public class ReplaceableLogOutput
    {
        public static ReplaceableLogOutput Instance { get; } = new ReplaceableLogOutput();

        private AutoResetEvent outputLocker = new AutoResetEvent(true);

        int lastType = -1;

        int outputLen = 0;

        int consoleWidth = Console.WindowWidth;

        int savedCursorTop = 0;

        private void ReplaceLog(string text, int type)
        {
            outputLocker.WaitOne();

            if (lastType != type || savedCursorTop != Console.CursorTop)
            {
                Console.WriteLine();
                outputLen = 0;
                lastType = type;
            }

            consoleWidth = Console.WindowWidth;


            var top = outputLen / consoleWidth;


            Console.SetCursorPosition(0, Console.CursorTop - top);

            savedCursorTop = Console.CursorTop;


            text = text.PadRight(outputLen > text.Length ? outputLen : text.Length);

            Console.Write(text);

            outputLen = text.Length;

            outputLocker.Set();
        }

        private void LineLog(string text)
        {
            outputLocker.WaitOne();

            lastType = -1;

            Console.WriteLine(text);

            outputLocker.Set();
        }
    }
}
