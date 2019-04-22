using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetScript
{
    public static class TypeHelper
    {
        public static void GenerateVariable(this Script script, string type, string name, object value = null)
        {
            GlobalVariable gv = new GlobalVariable();
            gv.name = name;
            gv.type = type;
            script.RegistrationGlobalVariable(gv);
        }

        public static void GenerateVariable(this Script script, Type type, string name, object value = null)
        {
            GlobalVariable gv = new GlobalVariable();
            gv.name = name;
            gv.type = type.ToString();
            script.RegistrationGlobalVariable(gv);
        }
    }
}
