//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace NSL.ShaderVM
//{
//    public class ShaderCompilationRequest
//    {
//        public MethodInfo Method { get; }

//        public ShaderEntryAttribute EntryAttribute { get; }

//        public IReadOnlyDictionary<string, ShaderFunctionAttribute> ShaderFunctions { get; }

//        public Type ExecutionContextType { get; }

//        public ShaderCompilationRequest(
//            MethodInfo method,
//            ShaderEntryAttribute entryAttribute,
//            IReadOnlyDictionary<string, ShaderFunctionAttribute> shaderFunctions,
//            Type executionContextType)
//        {
//            Method = method ?? throw new ArgumentNullException(nameof(method));
//            EntryAttribute = entryAttribute ?? throw new ArgumentNullException(nameof(entryAttribute));
//            ShaderFunctions = shaderFunctions ?? throw new ArgumentNullException(nameof(shaderFunctions));
//            ExecutionContextType = executionContextType ?? throw new ArgumentNullException(nameof(executionContextType));
//        }
//    }

//    //public class ShaderCompilationResult
//    //{
//    //    public string SourceCode { get; }

//    //    public IReadOnlyList<ShaderDiagnostic> Diagnostics { get; }

//    //    public bool Success { get; }

//    //    public ShaderCompilationResult(string sourceCode, IReadOnlyList<ShaderDiagnostic> diagnostics, bool success)
//    //    {
//    //        SourceCode = sourceCode ?? throw new ArgumentNullException(nameof(sourceCode));

//    //        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

//    //        Success = success;
//    //    }
//    //}

//    public class ShaderDiagnostic
//    {
//        public ShaderDiagnosticSeverity Severity { get; }

//        public string Message { get; }

//        public string FilePath { get; }

//        public int Line { get; }

//        public int Column { get; }

//        public ShaderDiagnostic(ShaderDiagnosticSeverity severity, string message, string filePath = null, int line = 0, int column = 0)
//        {
//            Severity = severity;
//            Message = message ?? throw new ArgumentNullException(nameof(message));
//            FilePath = filePath;
//            Line = line;
//            Column = column;
//        }

//        public override string ToString()
//        {
//            string loc = FilePath != null ? $"{FilePath}({Line},{Column})" : $"({Line},{Column})";
//            return $"{loc}: {Severity.ToString().ToLowerInvariant()}: {Message}";
//        }
//    }

//    public enum ShaderDiagnosticSeverity: byte
//    {
//        Info, Warning, Error
//    }

//    public class ShaderEntryInfo
//    {
//        public MethodInfo Method { get; }

//        public ShaderEntryAttribute Attribute { get; }

//        public IReadOnlyDictionary<string, ShaderFunctionAttribute> ShaderFunctions { get; }

//        public Type ExecutionContextType { get; }

//        public ShaderEntryInfo(
//            MethodInfo method,
//            ShaderEntryAttribute attribute,
//            IReadOnlyDictionary<string, ShaderFunctionAttribute> shaderFunctions,
//            Type executionContextType)
//        {
//            Method = method ?? throw new ArgumentNullException(nameof(method));
//            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
//            ShaderFunctions = shaderFunctions ?? throw new ArgumentNullException(nameof(shaderFunctions));
//            ExecutionContextType = executionContextType ?? throw new ArgumentNullException(nameof(executionContextType));
//        }
//    }
//}
