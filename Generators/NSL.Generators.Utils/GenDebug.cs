#if DEBUG || DEBUGEXAMPLES


using System.Diagnostics;

namespace NSL.Generators.Utils
{
    public class GenDebug
    {
        public static void Break(bool onlyAttach = false)
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();
            else if(!onlyAttach)
                Debugger.Break();
        }
    }
}

#endif