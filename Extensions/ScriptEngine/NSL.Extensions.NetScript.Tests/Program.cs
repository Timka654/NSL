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
            //try
            //{
            //    throw new Exception("abc");
            //}
            //catch (Exception ex)
            //{
            //    string test = ex.ToString();
            //}

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

                cscript.RegistrationReference("NetScript.dll");
                cscript.RegisterCoreReference("System.Linq.dll");
                cscript.RegisterCoreReference("System.ComponentModel.Primitives.dll");
                cscript.RegisterCoreReference("System.Diagnostics.Process.dll");

                cscript.RegisterExecutableReference();
                //foreach

                cscript.GenerateVariable(typeof(TScript),"Character");
                try
                {
                    cscript.Compile();
                }
                catch (NetScriptCompileException compileError)
                {
                    string code = cscript.DumpCompiledScriptText();

                    Console.WriteLine(compileError.ToString());
                    foreach (var item in compileError.Exceptions)
                    {
                        Console.WriteLine(item);
                    }
                    Console.ReadLine();
                }
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
