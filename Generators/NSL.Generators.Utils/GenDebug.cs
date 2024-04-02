#if DEBUG


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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