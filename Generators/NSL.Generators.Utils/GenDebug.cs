#if DEBUG || DEBUGEXAMPLES


using System.Diagnostics;

namespace NSL.Generators.Utils
{
    public class GenDebug
    {
        public static void Break()
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();
            else
                Debugger.Break();
        }
    }
}

#endif