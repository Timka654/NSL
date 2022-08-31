using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.NetScript.Example
{
    public class TScript
    {
        private static Script cscript;
        private Script script;


        public int id;
        public int ID { get { return id; } }

        public TScript(int id)
        {
            this.id = id;
            if (cscript == null)
            {
                cscript = new Script();
                cscript.AddFolder(Environment.CurrentDirectory + "\\scripts\\", true);

                //cscript.RegistrationReference("NetScript.dll");
                cscript.RegisterCoreReference("System.Linq.dll");
                cscript.RegisterCoreReference("System.ComponentModel.Primitives.dll");
                cscript.RegisterCoreReference("System.Diagnostics.Process.dll");

                cscript.RegisterExecutableReference();
                //foreach

                cscript.GenerateVariable(typeof(TScript), "Character");
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
#if DEBUG
                cscript.OnPerformanceMessageEvent += Cscript_OnPerformanceMessageEvent;
#endif
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
            Console.WriteLine(string.Format("CallCount {0} for id:{1}", script.InvokeMethod<int>("ScriptCore", "GetCallCount", script.GetCoreObject()), id));
        }

        public void Debug(string value)
        {
            Console.WriteLine(value);
        }
    }
}
