using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NSL.Refactoring.FastAction.Core
{
    internal class FastActionData
    {
        public bool PreventCache { get; set; } = false;

        public ActionData[] Actions { get; set; } = Array.Empty<ActionData>();

        public GroupData[] Groups { get; set; } = Array.Empty<GroupData>();
    }


}
