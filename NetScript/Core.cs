using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.IO;

namespace NetScript
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

        private string libFilePath = Path.GetTempFileName();

        /// <summary>
        /// Подключаемые библиотеки
        /// </summary>
        internal List<string> references = new List<string>();

        /// <summary>
        /// using
        /// </summary>
        internal List<string> usings = new List<string>();

        internal List<CompileCodeFragmentInfo> Fragments = new List<CompileCodeFragmentInfo>();

        /// <summary>
        /// Глобальные переменные 
        /// </summary>
        internal List<GlobalVariable> globals = new List<GlobalVariable>();

        internal Dictionary<string, Dictionary<string, MethodInfo>> CacheMethodMap { get; set; }

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

            StringBuilder sb = new StringBuilder();
            //добавляем в код using
            foreach (var item in usings)
            {
                this.code = string.Format("using {0};\r\n", item) + this.code;
            }
            //создаем класс для глобальный переменных
            sb.AppendLine("public class Globals {");
            foreach (var item in globals)
            {
                sb.AppendLine($"public {item.type} {item.name} {{ get; set; }}");
            }
            sb.AppendLine("}");
            //Добавляем класс с глобальными переменными в код
            this.code += sb.ToString();

        }

        /// <summary>
        /// Компиляция скриптов
        /// </summary>
        internal void Compile()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release);

            PreCompile(options);

            var source = SourceText.From(this.code, Encoding.UTF8);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(source, options: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8));


            var defaultReferences = references.Select(x => MetadataReference.CreateFromFile(x));

            var compilation
                = CSharpCompilation.Create(Assembly.GetCallingAssembly().FullName, new SyntaxTree[] { syntaxTree }, defaultReferences, options);

            var result = compilation.Emit(libFilePath);

            if (!result.Success)
            {
                throw new NetScriptCompileException("Ошибка во время компиляции",
                this, result.Diagnostics);
            }

            CacheMethodMap = new Dictionary<string, Dictionary<string, MethodInfo>>();

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
            if (!CacheMethodMap.ContainsKey(_class))
            {
                CacheMethodMap.Add(_class, new Dictionary<string, MethodInfo>());
            }
            if (!CacheMethodMap[_class].ContainsKey(_method))
            {
                CacheMethodMap[_class].Add(_method, lib.GetType(_class).GetMethod(_method));
            }
            return CacheMethodMap[_class][_method].Invoke(_obj, args);
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
