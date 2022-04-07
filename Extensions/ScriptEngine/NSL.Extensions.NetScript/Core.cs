using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace NSL.Extensions.NetScript
{
    internal class Core : ICloneable
    {
        internal PropertyInfo GlobalField;

        /// <summary>
        /// Установленый код
        /// </summary>
        internal string code = string.Empty;

        /// <summary>
        /// Скомпилирована библиотека
        /// </summary>
        internal Assembly lib;

        /// <summary>
        /// Входная точка Core
        /// </summary>
        internal object scriptCoreData;

        internal object globalData;

        internal int GlobalCodeStartLine;
        internal int GlobalCodeEndLine;

        private string libFilePath = Path.GetTempFileName();

        /// <summary>
        /// Подключаемые библиотеки
        /// </summary>
        internal List<string> references = new List<string>();

        /// <summary>
        /// using
        /// </summary>
        internal List<string> usings = new List<string>();

        /// <summary>
        /// #define
        /// </summary>
        internal List<string> defines = new List<string>();

        internal List<CompileCodeFragmentInfo> Fragments = new List<CompileCodeFragmentInfo>();

        /// <summary>
        /// Глобальные переменные 
        /// </summary>
        internal List<GlobalVariable> globals = new List<GlobalVariable>();

        internal Dictionary<int, Dictionary<int, MethodInfo>> CacheMethodMap { get; set; }

        internal Dictionary<string, Dictionary<string, PropertyInfo>> CachePropertyMap { get; set; }

        /// <summary>
        /// Установка кода скрипта
        /// </summary>
        /// <param name="code"></param>
        internal void SetCode(string code)
        {
            this.code = code;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        internal Core()
        {

        }

        /// <summary>
        /// Клонирование скомпилированого скрипта с новым обьектом глобальных переменных
        /// </summary>
        /// <returns>Новый экземпляр Core</returns>
        public object Clone()
        {
            //Обявление новой переменной ядра
            Core c = new Core
            {
                //Копирование компилированной библитеки
                lib = this.lib,
                //Инициализация главного класса ScriptCore
                scriptCoreData = Activator.CreateInstance(lib.GetType("ScriptCore"))
            };

            c.InitializeCore();

            //Инициализация переменных
            c.InitializeGlobal();
            
            return c;
        }

        /// <summary>
        /// Предкомпиляция деректив using и глобальный переменных
        /// </summary>
        /// <param name="cp"></param>
        private void PreCompile(CSharpCompilationOptions cp)
        {
            cp.WithUsings(usings);

            //добавляем перед кодом using
            if (usings.Any())
            {
                foreach (var item in usings)
                {
                    this.code = string.Format("using {0};\r\n", item) + this.code;
                }
            }

            //добавляем перед кодом define
            if (defines.Any())
            {
                foreach (var item in defines)
                {
                    this.code = string.Format("#define {0}\r\n", item) + this.code;
                }
            }

            StringBuilder sb = new StringBuilder();

            //создаем класс для глобальный переменных
            sb.AppendLine("public class Globals {");
            foreach (var item in globals)
            {
                sb.AppendLine($"public {item.type} {item.name} {{ get; set; }}");
            }
            sb.AppendLine("}");

            GlobalCodeStartLine = this.code.Split("\n").Count();

            //Добавляем класс с глобальными переменными в код
            this.code += sb.ToString();

            GlobalCodeEndLine = GlobalCodeStartLine + sb.ToString().Split("\n").Count();

        }

        /// <summary>
        /// Компиляция скриптов
        /// </summary>
        internal void Compile()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release);


            var dd = typeof(Object).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            PreCompile(options);

            var source = SourceText.From(this.code, Encoding.UTF8);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(source, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));


            var defaultReferences = references
                .Select(x => MetadataReference.CreateFromFile(x))
                .Append(MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location))
                .Append(MetadataReference.CreateFromFile(Path.Combine(coreDir.FullName, "mscorlib.dll")))
                .Append(MetadataReference.CreateFromFile(Path.Combine(coreDir.FullName, "System.Runtime.dll")));

            var compilation
                = CSharpCompilation.Create(Assembly.GetCallingAssembly().FullName, new SyntaxTree[] { syntaxTree }, defaultReferences, options);

            var result = compilation.Emit(libFilePath);

            if (!result.Success)
            {
                throw new NetScriptCompileException("Ошибка во время компиляции",
                this, result.Diagnostics);
            }

            CacheMethodMap = new Dictionary<int, Dictionary<int, MethodInfo>>();

            CachePropertyMap = new Dictionary<string, Dictionary<string, PropertyInfo>>();


            //устанавливаем результат
            lib = Assembly.LoadFile(libFilePath);

            // Создаем переменную главного класса - ScriptCore 
            scriptCoreData = Activator.CreateInstance(lib.GetType("ScriptCore"));

            //Инициализируем
            InitializeCore();

            //Устанавливаем глобальные переменные
            InitializeGlobal();
        }

        internal object InvokeMethod(string _class, string _method, object _obj, params object[] args)
        {
            var method = GetMethod(_class, _method);

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return InvokeMethod(method, _obj, args);
        }

        internal MethodInfo GetMethod(string _class, string _method)
        {
            if (!CacheMethodMap.TryGetValue(_class.GetHashCode(), out var methodMap))
            {
                methodMap = new Dictionary<int, MethodInfo>();

                CacheMethodMap.Add(_class.GetHashCode(), methodMap);
            }
            if (!methodMap.TryGetValue(_method.GetHashCode(), out var method))
            {
                method = lib.GetType(_class).GetMethod(_method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                methodMap.Add(_method.GetHashCode(), method);
            }

            return method;
        }

        internal object InvokeMethod(MethodInfo _method, object _obj, params object[] args)
        {
            try
            {
                return _method.Invoke(_obj, args);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex}\r\n   at Class = {_method.DeclaringType.Name}\r\n   at Method = {_method.Name}\r\n   with args =  {System.Text.Json.JsonSerializer.Serialize(args)}");
            }
        }

        internal void SetProperty(string _class, string _property, object _obj, object value)
        {
            if (!CachePropertyMap.ContainsKey(_class))
            {
                CachePropertyMap.Add(_class, new Dictionary<string, PropertyInfo>());
            }
            if (!CachePropertyMap[_class].ContainsKey(_property))
            {
                CachePropertyMap[_class].Add(_property, lib.GetType(_class).GetProperty(_property));
            }
            CachePropertyMap[_class][_property].SetValue(_obj, value);
        }

        internal object GetProperty(string _class, string _property, object _obj)
        {
            if (!CachePropertyMap.ContainsKey(_class))
            {
                CachePropertyMap.Add(_class, new Dictionary<string, PropertyInfo>());
            }
            if (!CachePropertyMap[_class].ContainsKey(_property))
            {
                CachePropertyMap[_class].Add(_property, lib.GetType(_class).GetProperty(_property));
            }
            return CachePropertyMap[_class][_property].GetValue(_obj);
        }

        /// <summary>
        /// Инициализация глобальных значений
        /// </summary>
        internal void InitializeGlobal()
        {
            //Получаем в главном классе имя переменной содержащей глобальные значения
            var globalFieldName = lib.GetType("ScriptCore").GetField("globalMember");
            if (globalFieldName == null)
                throw new NetScriptCompileException("Отсутствует обязательная публичная константа имени переменной \"globalMember\" типа string");

            string globalVariableName = (string)globalFieldName.GetValue(scriptCoreData);

            //получаем переменную для установки глобальных значений
            GlobalField = lib.GetType("ScriptCore").GetProperty(globalVariableName);
            if (GlobalField == null)
                throw new NetScriptCompileException($"Отсутствует обязательный публичный метод {globalFieldName} определен в публичной константе globalMember типа string {globalVariableName}\r\n");


            globalData = null;
            try
            {
                //инициализируем переменную глобальных значений
                globalData = GlobalField.GetValue(scriptCoreData);
                if (globalData == null)
                {
                    globalData = Activator.CreateInstance(lib.GetType("Globals"));
                    GlobalField.SetValue(scriptCoreData, globalData);
                }

            }
            catch (Exception ex)
            {
                throw new NetScriptCompileException($"Ошибка вызова метода {globalVariableName} определен в публичной константе getGlobalMethod, метод должен буть публичным, не может иметь аргументы и должен возвращать переменную типа Globals, подробнее: {ex.ToString()}\r\n");
            }

            //еще раз получаем переменную глобальных значений
            globalData = GlobalField.GetValue(scriptCoreData);

            var gvv = globals.Where(x => x.value != null).ToList();
            //устанавливаем значения
            foreach (var item in gvv)
            {
                var fl = globalData.GetType().GetProperty(item.name);
                fl.SetValue(globalData, item.value);
            }
        }

        /// <summary>
        /// Инициализация скрипта
        /// </summary>
        internal void InitializeCore()
        {
            //вызываем InitMethod класса ScriptCore
            var initMethodMemberName = lib.GetType("ScriptCore").GetField("initMethod");

            if (initMethodMemberName == null)
                throw new NetScriptCompileException($"Отсутствует обязательная публичная константа имени метода \"initMethod\" типа string\r\n");


            string initMethodName = (string)initMethodMemberName.GetValue(scriptCoreData);

            var globalMethod = lib.GetType("ScriptCore").GetMethod(initMethodName);

            if (globalMethod == null)
                throw new NetScriptCompileException($"Отсутствует обязательный публичный метод {initMethodName} определен в публичной константе InitMethod типа string\r\n");

            try
            {
                globalMethod.Invoke(scriptCoreData, null);
            }
            catch (Exception ex)
            {
                throw new NetScriptCompileException($"Ошибка вызова метода {initMethodMemberName.GetValue(scriptCoreData)} определен в публичной константе InitMethod, метод должен быть публичным, не может иметь аргументы и должен возвращать переменную типа Globals, подробнее: {ex.ToString()}\r\n");
            }

        }
    }
}
