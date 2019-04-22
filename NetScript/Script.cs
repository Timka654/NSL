using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetScript
{
    public class Script
    {
#if DEBUG

        private System.Diagnostics.Stopwatch performanceTimer = new System.Diagnostics.Stopwatch();

        public event EventHandler<string> OnPerformanceMessageEvent;

#endif

        /// <summary>
        /// Ядро скрипта
        /// </summary>
        private Core core;

        /// <summary>
        /// Код скрипта
        /// </summary>
        private string code = "#define CompileScript\r\n";

        /// <summary>
        /// Конструктор
        /// </summary>
        public Script()
        {
            core = new Core();
        }

        /// <summary>
        /// Поиск и добавление скриптов в указаном каталоге
        /// </summary>
        /// <param name="folder">Путь к каталогу</param>
        /// <param name="subFolders">Включение всех подкаталогов</param>
        public void AddFolder(string folder, bool subFolders)
        {
            //получаем все файлы
            foreach (var item in Directory.GetFiles(folder, "*.cs"))
            {
                AddFile(item);
            }
            //если нужно, проверяем поддериктории
            if (subFolders)
            {
                foreach (var item in Directory.GetDirectories(folder))
                {
                    AddFolder(item, subFolders);
                }
            }
        }

        /// <summary>
        /// Добавить файл скрипта
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public void AddFile(string path)
        {
            int start = code.Count(x => x == '\n');

            using (StreamReader sr = new StreamReader(path))
            {
                code += sr.ReadToEnd() + "\r\n";
            }

            int end = code.Count(x=> x == '\n');

            core.Fragments.Add(new CompileCodeFragmentInfo() { Start = start, End = end, FileName = path });
        }

        /// <summary>
        /// Компиляция, только после регистрации всех библиотек и using 
        /// </summary>
        public void Compile()
        {
            core.SetCode(code);
            core.Compile();
        }

        /// <summary>
        /// Регистрация библиотеки
        /// </summary>
        /// <param name="library">Имя библиотеки прим. System.dll System.Xml.dll ...</param>
        public void RegistrationReference(string library)
        {
            core.references.Add(library);
        }

        /// <summary>
        /// Регистрация дерективы using
        /// </summary>
        /// <param name="_using">имя using прим. System, System.IO ...</param>
        public void RegistrationUsingDirective(string _using)
        {
            core.usings.Add(_using);
        }

        /// <summary>
        /// Регистрация глобальной переменной
        /// </summary>
        /// <param name="globalvar"></param>
        public void RegistrationGlobalVariable(GlobalVariable globalvar)
        {
            core.globals.Add(globalvar);
        }

        /// <summary>
        /// Установка значения глобальной переменной
        /// </summary>
        /// <param name="_var">Имя переменной</param>
        /// <param name="value">Значение переменной</param>
        public void SetGlobalVariable(string _var, object value)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();
#endif
            SetProperty("Globals", _var, core.globalData, value);
#if DEBUG
            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"SetGlobalVariable elapsed = {performanceTimer.ElapsedMilliseconds}");
#endif
        }

        public T GetGlobalVariable<T>(string _var, object value)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();

            var r = GetProperty<T>("Globals", _var, core.globalData);

            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"GetGlobalVariable elapsed = {performanceTimer.ElapsedMilliseconds}");

            return r;
#else
            return GetProperty<T>("Globals", _var, core.globalData);
#endif
        }

        public T GetProperty<T>(string _class, string _property, object _obj)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();

            T r = (T)core.GetProperty(_class, _property, _obj);

            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"GetProperty elapsed = {performanceTimer.ElapsedMilliseconds}");

            return r;
#else
            return (T)core.GetProperty(_class, _property, _obj);
#endif
        }

        public void SetProperty(string _class, string _property, object _obj, object value)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();
#endif
            core.SetProperty(_class, _property, _obj, value);
#if DEBUG
            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"SetProperty elapsed = {performanceTimer.ElapsedMilliseconds}");
#endif
        }

        public T InvokeMethod<T>(string _class, string _method, object _obj, params object[] args)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();

            T r = (T)core.InvokeMethod(_class, _method, _obj, args);

            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"InvokeMethod elapsed = {performanceTimer.ElapsedMilliseconds}");

            return r;
#else
            return (T)core.InvokeMethod(_class, _method, _obj, args);
#endif
        }

        public void InvokeMethod(string _class, string _method, object _obj, params object[] args)
        {
#if DEBUG
            performanceTimer.Reset();
            performanceTimer.Start();
#endif
            core.InvokeMethod(_class, _method, _obj, args);
#if DEBUG
            performanceTimer.Stop();
            OnPerformanceMessageEvent?.Invoke(this, $"InvokeMethod elapsed = {performanceTimer.ElapsedMilliseconds}");
#endif
        }

        public System.Reflection.Assembly GetAssembly()
        {
            return core.lib;
        }

        public object GetGlobalObject()
        {
            return core.globalData;
        }

        public object GetCoreObject()
        {
            return core.scriptCoreData;
        }

        /// <summary>
        /// Клонирование скомпилированого скрипта с новым обьектом глобальных переменных
        /// </summary>
        /// <returns>Новый экземпляр Core</returns>
        public Script Copy()
        {
            Script ns = new Script();
#if DEBUG
            ns.OnPerformanceMessageEvent += OnPerformanceMessageEvent;
#endif
            ns.core = (Core)this.core.Clone();
            ns.core.CacheMethodMap = new Dictionary<string, Dictionary<string, System.Reflection.MethodInfo>>();
            ns.core.CachePropertyMap = new Dictionary<string, Dictionary<string, System.Reflection.PropertyInfo>>();

            ns.core.globals = core.globals;
            ns.core.references = core.references;
            ns.core.usings = core.usings;
            ns.core.Fragments = core.Fragments;

            ns.core.code = core.code;

            return ns;
        }

        public string DumpScriptText()
        {
            return this.code;
        }

        public string DumpCoreCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("public class ScriptCore");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic const string globalMember = \"GlobalData\";");
            sb.AppendLine("\tpublic const string initMethod = \"Initialize\";");
            sb.AppendLine();
            sb.AppendLine("\tpublic Globals GlobalData { get; set; }");
            sb.AppendLine();
            sb.AppendLine("\tpublic static ScriptCore Instance;");
            sb.AppendLine();
            sb.AppendLine("\tpublic void Initialize()");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tInstance = this;");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();

        }

        public string DumpGlobalCode()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("#if !CompileScript");
            sb.AppendLine("public class Globals");
            sb.AppendLine("{");
            foreach (var item in core.globals)
            {
                sb.AppendLine($"\tpublic {item.type} {item.name} {{ get; set; }}");
            }
            sb.AppendLine("}");
            sb.AppendLine("#endif");

            return sb.ToString();
        }
    }
}
