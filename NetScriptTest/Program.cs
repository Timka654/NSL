using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NetScript;

namespace NetScriptTest
{
    class Program
    {
        static Dictionary<int, TScript> scripts = new Dictionary<int, TScript>();
        static void Main(string[] args)
        {
            scripts.Add(0, new TScript(0));
            scripts.Add(1, new TScript(1));
            Random r = new Random();
            for (int i = 0; i < 21; i++)
            {
                scripts[r.Next(0, 2)].TestCall();
            }
            scripts[0].ShowCallCount();
            scripts[1].ShowCallCount();
            Console.ReadKey();
        }
    }
    public class TScript
    {
        private static NetScript.Script cscript;
        private NetScript.Script script;


        public int id;
        public int ID { get { return id; } }

        public TScript(int id)
        {
            this.id = id;
            if (cscript == null)
            {
                cscript = new NetScript.Script();
                cscript.AddFolder(Environment.CurrentDirectory + "\\scripts\\", true);
                cscript.RegistrationReference("mscorlib.dll");
                cscript.RegistrationReference("System.dll");
                cscript.RegistrationReference("System.Runtime.dll");
                cscript.RegistrationReference("System.Private.CoreLib.dll");
                cscript.RegistrationReference("System.Linq.dll");
                cscript.RegistrationReference(System.Reflection.Assembly.GetAssembly(typeof(Program)).Location);
                cscript.RegistrationUsingDirective("System");
                cscript.RegistrationUsingDirective("System.Collections.Generic");
                cscript.RegistrationUsingDirective("System.Linq");
                cscript.RegistrationUsingDirective("System.Text");
                cscript.GenerateVariable(typeof(TScript),"Character");
                cscript.Compile();
                cscript.OnPerformanceMessageEvent += Cscript_OnPerformanceMessageEvent;
            }
            script = cscript.Copy();
            script.SetGlobalVariable("Character", this);
        }

        private void Cscript_OnPerformanceMessageEvent(object sender, string e)
        {
            Console.WriteLine(e);
        }

        public void TestCall()
        {
            Debug(string.Format("testCall id = {0}", id));
            script.InvokeMethod("ScriptCore", "testCall", script.GetCoreObject(), null);
        }
        public void ShowCallCount()
        { 
            Console.WriteLine(string.Format("CallCount {0} for id:{1}",script.InvokeMethod<int>("ScriptCore", "GetCallCount", script.GetCoreObject()),id));
        }
        public void Debug(string value)
        {
            Console.WriteLine(value);
        }
    }
}
