﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.HttpEndPointGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NSL.Generators.HttpEndPointGenerator
{
    [Generator]
    internal class HttpEndPointGenerator : ISourceGenerator
    {
        #region ISourceGenerator

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is HttpEndPointImplementAttributeSyntaxReceiver methodSyntaxReceiver)
            {
                ProcessHttpEndPoints(context, methodSyntaxReceiver);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new HttpEndPointImplementAttributeSyntaxReceiver());
        }

        #endregion

        private void ProcessHttpEndPoints(GeneratorExecutionContext context, HttpEndPointImplementAttributeSyntaxReceiver methodSyntaxReceiver)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            foreach (var item in methodSyntaxReceiver.Types)
            {
                ProcessHttpEndPoint(context, item);
            }
        }

        private void ProcessHttpEndPoint(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            if (!type.HasPartialModifier())
                return;

            var typeClass = type as ClassDeclarationSyntax;

            var typeSem = context.Compilation.GetSemanticModel(typeClass.SyntaxTree);

            var classBuilder = new CodeBuilder();

            classBuilder.AddUsing("DevExtensions.Blazor.Http");

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL HttpEndPoint Generator. Please don't change this file");
            });

            classBuilder.CreatePartialClass(typeClass, () =>
            {
                classBuilder.AppendLine($"protected partial HttpClient CreateEndPointClient(string url);");

                classBuilder.AppendLine();


                var urlDeclarations = new List<string>();

                var methodDeclarations = new List<string>();

                var attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(ImplementGenerateAttributeFullName))
                .ToArray();

                foreach (var attr in attrbs)
                {
                    //if (!Debugger.IsAttached)
                    //    Debugger.Launch();

                    var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

                    var fillArgs = attr.ArgumentList.Arguments;

                    var containerType = fillArgs.First().GetAttributeTypeParameterValueSymbol(typeSem);

                    var containerAttributes = containerType.GetAttributes();

                    var containerAttr = containerAttributes.FirstOrDefault(x => x.AttributeClass.Name.Equals(ContainerGenerateAttributeFullName));

                    var controllerName = GetControllerName(containerType);

                    string prefixUrl = containerAttr?.ConstructorArguments.FirstOrDefault().Value as string;

                    if (prefixUrl == null)
                    {
                        var tryRouteAttr = containerAttributes.FirstOrDefault(x => x.AttributeClass.Name.Equals(RouteAttributeFullName));

                        if (tryRouteAttr != null)
                            prefixUrl = tryRouteAttr?.ConstructorArguments.FirstOrDefault().Value as string;

                        if (prefixUrl == null)
                        {
                            prefixUrl = controllerName;
                        }
                    }

                    prefixUrl = prefixUrl.Replace("[controller]", controllerName);

                    var prefixVarName = $"{controllerName}PrefixUrl";

                    classBuilder.AppendLine($"protected const string {prefixVarName} = \"{prefixUrl}\";");

                    classBuilder.AppendLine();

                    var members = containerType.GetAllMembers();

                    foreach (var item in members)
                    {
                        //if (!Debugger.IsAttached)
                        //    Debugger.Launch();

                        if (!(item is IMethodSymbol ms))
                            continue;

                        var mparam = ms.Parameters.FirstOrDefault();

                        var mattrbs = ms.GetAttributes();

                        var genAttribute = mattrbs.FirstOrDefault(x => x.AttributeClass.Name.Equals(GenerateAttributeFullName));

                        var returnType = genAttribute?.ConstructorArguments.FirstOrDefault().Value as ITypeSymbol;

                        var url = genAttribute?.ConstructorArguments.ElementAtOrDefault(1).Value as string;

                        if (url == default)
                        {
                            var templates = mattrbs.Select(x =>
                            {
                                var param = x.AttributeConstructor.Parameters.FirstOrDefault(q => q.Name == "template");

                                if (param == null)
                                    return default;

                                var idx = x.AttributeConstructor.Parameters.IndexOf(param);

                                var val = x.ConstructorArguments[idx].Value as string;

                                return (val, x.AttributeClass.Name);
                            }).Where(x => x != default).ToArray();

                            if(!templates.Any())
                                templates = new(string,string)[] { ("[action]", "Post") };

                            foreach (var templ in templates)
                            {
                                var _t = GetHttpRequestType(templ.Name);
                                var _vname = $"{controllerName}{ms.Name}{_t}";

                                urlDeclarations.Add($"protected const string {_vname}Url = $\"{{{prefixVarName}}}/{templ.val.Replace("[action]", ms.Name)}\";");

                                List<string> _p = new List<string>();

                                if (mparam != null)
                                    _p.Add($"{mparam.Type} {mparam.Name}");

                                _p.Add($"{BaseHttpRequestOptionsFullName} __options = null");


                                CodeBuilder methodBuilder = new CodeBuilder();

                                methodBuilder.AppendLine($"public async Task<{returnType}> {_vname}Request({string.Join(", ", _p)})");
                                methodBuilder.NextTab();
                                methodBuilder.AppendLine($"=> await CreateEndPointClient({_vname}Url)");
                                if (_t == "Post")
                                {
                                    if(mparam == null)
                                        methodBuilder.AppendLine($".PostEmptyAsync({_vname}Url)");
                                    else
                                        methodBuilder.AppendLine($".PostJsonAsync({_vname}Url, {mparam.Name})");
                                }

                                methodBuilder.AppendLine($".ProcessResponseAsync<{returnType}>(__options);");

                                methodDeclarations.Add(methodBuilder.ToString());
                            }
                        }
                    }

                }
#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
                    classBuilder.AppendLine(string.Join($"{Environment.NewLine}{Environment.NewLine}", urlDeclarations));
                    classBuilder.AppendLine();
                    classBuilder.AppendLine(string.Join($"{Environment.NewLine}{Environment.NewLine}", methodDeclarations));
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
            });

            // Visual studio have lag(or ...) cannot show changes sometime
            //#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //System.IO.File.WriteAllText($@"C:\Work\temp\{typeClass.GetTypeClassName()}..httpendpoints.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            context.AddSource($"{typeClass.GetTypeClassName()}.httpendpoints.cs", classBuilder.ToString());
        }

        private string GetControllerName(ITypeSymbol containerType)
        {
            var prefixUrl = containerType.Name;

            if (prefixUrl.StartsWith("I"))
                prefixUrl = prefixUrl.Substring(1);

            if (prefixUrl.EndsWith("Controller"))
                prefixUrl = prefixUrl.Substring(0, prefixUrl.Length - "Controller".Length);

            return prefixUrl;
        }

        private string GetHttpRequestType(string attributeName)
        {

            if (attributeName.Contains("Get"))
                return "Get";

            if (attributeName.Contains("Put"))
                return "Put";

            if (attributeName.Contains("Post"))
                return "Post";

            if (attributeName.Contains("DELETE"))
                return "Delete";

            return "Unk";
        }


        private readonly string ImplementGenerateAttributeFullName = typeof(HttpEndPointImplementGenerateAttribute).Name;
        private readonly string ContainerGenerateAttributeFullName = typeof(HttpEndPointContainerGenerateAttribute).Name;
        private readonly string GenerateAttributeFullName = typeof(HttpEndPointGenerateAttribute).Name;

        private readonly string RouteAttributeFullName = "RouteAttribute";
        private readonly string BaseHttpRequestOptionsFullName = "BaseHttpRequestOptions";
    }
}
