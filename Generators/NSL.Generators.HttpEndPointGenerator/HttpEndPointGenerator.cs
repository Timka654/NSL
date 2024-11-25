﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes;
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
#if DEBUG
            //GenDebug.Break();

#endif
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

            classBuilder.AddUsing("NSL.HttpClient");
            classBuilder.AddUsing("NSL.HttpClient.HttpContent");
            classBuilder.AddUsing("NSL.HttpClient.Models");
            classBuilder.AddUsing("NSL.HttpClient.Validators");

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL HttpEndPoint Generator. Please don't change this file");
            });

            classBuilder.CreatePartialClass(typeClass, () =>
            {
                classBuilder.AppendLine($"protected partial System.Net.Http.HttpClient CreateEndPointClient(string url);");

                classBuilder.AppendLine();


                var urlDeclarations = new List<string>();

                var methodDeclarations = new List<string>();

                var attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(ImplementGenerateAttributeFullName))
                .ToArray();

                foreach (var attr in attrbs)
                {
                    //GenDebug.Break();

                    var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

                    var fillArgs = attr.ArgumentList.Arguments;

                    var containerType = fillArgs.First().GetAttributeTypeParameterValueSymbol(typeSem);

                    var saveNames = fillArgs.ElementAtOrDefault(1)?.GetAttributeParameterValue<bool>(typeSem) ?? false;

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
                        //GenDebug.Break();

                        if (!(item is IMethodSymbol ms))
                            continue;


                        var mparams = ms.Parameters.Where(x => x.GetAttributes().Any(a =>
                        a.AttributeClass.Name.Equals(FromBodyAttributeFullName) ||
                        a.AttributeClass.Name.Equals(FromFormAttributeFullName) || a.AttributeClass.Name.Equals(ParameterAttributeFullName))).ToImmutableArray();




                        var mattrbs = ms.GetAttributes();

                        var genAttribute = mattrbs.FirstOrDefault(x => x.AttributeClass.Name.Equals(GenerateAttributeFullName));

                        if (genAttribute == null)
                            continue;

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

                            if (!templates.Any())
                                templates = new (string, string)[] { ("[action]", "Post") };

                            foreach (var templ in templates)
                            {
                                var _t = GetHttpRequestType(templ.Name);
                                var _vname = $"{controllerName}{ms.Name}{_t}";

                                urlDeclarations.Add($"protected const string {_vname}Url = $\"{{{prefixVarName}}}/{templ.val.Replace("[action]", ms.Name)}\";");

                                List<string> _p = new List<string>();

                                _p.AddRange(GetParameter(mparams));

                                _p.Add($"{BaseHttpRequestOptionsFullName} __options = null");


                                CodeBuilder methodBuilder = new CodeBuilder();

                                methodBuilder.AppendSummary(b =>
                                {
                                    b.AppendSummaryLine($"Generate for <see cref=\"{containerType.Name}.{ms.Name}({string.Join(", ", ms.Parameters.Select(x => x.Type.ToString().Replace('<', '{').Replace('>', '}')))})\"/>");

                                });
#if DEBUG
                                //GenDebug.Break();
#endif

                                var requestMethodName = saveNames ? ms.Name : $"{_vname}Request";

                                methodBuilder.AppendLine($"public async Task<{returnType}> {requestMethodName}({string.Join(", ", _p)})");
                                methodBuilder.NextTab();
                                methodBuilder.AppendLine($"=> await CreateEndPointClient({_vname}Url)");
                                methodBuilder.AppendLine($".FillClientOptions(__options)");
                                if (_t == "Post")
                                {
                                    if (!mparams.Any())
                                        methodBuilder.AppendLine($".PostEmptyAsync({_vname}Url)");
                                    //if (mparams.Length == 1 && !mparams[0].GetAttributes().Any(x=>x.AttributeClass.Name.Equals(ParameterAttributeFullName) && (GenHttpParameterEnum)x.ConstructorArguments[0].Value != GenHttpParameterEnum.Particle))
                                    //    methodBuilder.AppendLine($".PostJsonAsync({_vname}Url, {mparams[0].Name})");
                                    else
                                    {
                                        methodBuilder.AppendLine($".PostBuildAsync({_vname}Url, ()=>{{");
                                        methodBuilder.NextTab();
                                        methodBuilder.AppendLine("var ____content = ");

                                        BuildContent(mparams.Cast<ISymbol>().ToImmutableArray(), methodBuilder);

                                        methodBuilder.AppendLine("return ____content;");
                                        methodBuilder.PrevTab();
                                        //JsonHttpContent.Create(data)
                                        methodBuilder.AppendLine("})");
                                    }
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
            //System.IO.File.WriteAllText($@"D:\Projects\work\my\NSL\NSL\Generators\Tests\HttpEndPoint\NSL.Generators.HttpEndPointGenerator.Tests.Client\{typeClass.GetTypeClassName()}.httpendpoints.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            context.AddSource($"{typeClass.GetTypeClassName()}.httpendpoints.cs", classBuilder.ToString());
        }

        private List<string> GetParameter(IEnumerable<ISymbol> parameters, string path = null)
        {
            List<string> result = new List<string>();

            foreach (var mparam in parameters)
            {
                var type = mparam.GetTypeSymbol();

                var t = type.ToString();

                var name = mparam.Name;

                if (type.Name.Equals(IFormFileFullName))
                {
                    t = "NSL.HttpClient.HttpContent.StreamDataContent";
                    name = string.Join("__", path, name).TrimStart('_');
                }
                else if (type.Name.Equals(IFormFileCollectionFullName))
                {
                    t = "IEnumerable<NSL.HttpClient.HttpContent.StreamDataContent>";
                    name = string.Join("__", path, name).TrimStart('_');
                }
                else if (path != null)
                {
                    //unprocess - continue
                    continue;
                }

                result.Add($"{t} {name}");

                if (mparam.GetAttributes().Any(x => x.AttributeClass.Name.Equals(FromFormAttributeFullName)) && mparam.GetAttributes().Any(x => x.AttributeClass.Name.Equals(ParameterAttributeFullName) && ((GenHttpParameterEnum)x.ConstructorArguments[0].Value) == GenHttpParameterEnum.Particle))
                {
                    result.AddRange(GetParameter(type.GetAllMembers().Where(x => x is IPropertySymbol).ToImmutableArray(), name));
                }
            }

            return result;
        }

        private void BuildContent(ImmutableArray<ISymbol> mparams, CodeBuilder methodBuilder)
        {
            var firstAttributes = mparams[0].GetAttributes();

            if (!mparams.Any(x =>
            x.GetAttributes().Any(a => a.AttributeClass.Name.Equals(FromFormAttributeFullName) || a.AttributeClass.Name.Equals(FromBodyAttributeFullName))))
            {
                methodBuilder.AppendLine($"EmptyHttpContent.Create();");

                FillHeaders(mparams, methodBuilder);
            }
            else if (mparams.Length == 1 && firstAttributes.Any(x => x.AttributeClass.Name.Equals(FromBodyAttributeFullName)))
            {
                var _pa = firstAttributes.FirstOrDefault(x => x.AttributeClass.Name.Equals(ParameterAttributeFullName));

                if (_pa == null || (GenHttpParameterEnum)_pa.ConstructorArguments[0].Value == GenHttpParameterEnum.Normal)
                {
                    methodBuilder.AppendLine($"JsonHttpContent.Create({mparams[0].Name});");

                    FillHeaders(mparams, methodBuilder);
                }
            }
            else if (mparams.Any(x => x.GetAttributes().Any(b => b.AttributeClass.Name.Equals(FromFormAttributeFullName))))
            {
                methodBuilder.AppendLine($"FormHttpContent.Create();");

                FillHeaders(mparams, methodBuilder);

                BuildForm(mparams, methodBuilder, null);
            }
            else
            {

            }
        }

        private void FillHeaders(ImmutableArray<ISymbol> mparams, CodeBuilder methodBuilder)
        {
            foreach (var item in mparams)
            {
                var itemAttrbs = item.GetAttributes();

                var fromHeaderAttribute = itemAttrbs.FirstOrDefault(x => x.AttributeClass.Name.Equals(FromHeaderAttributeFullName));

                if (fromHeaderAttribute == null)
                    continue;

                var formHeaderValue = fromHeaderAttribute?.GetNamedArgumentValue(nameof(FromHeaderAttribute.Name));

                if (formHeaderValue is TypedConstant tc)
                    formHeaderValue = tc.Value;

                methodBuilder.AppendLine($"____content.Headers.Add(\"{formHeaderValue ?? item.Name}\", {item.Name});");
            }
        }

        private void BuildForm(ImmutableArray<ISymbol> mparams, CodeBuilder methodBuilder, string path)
        {
            foreach (var item in mparams)
            {
                var endPath = string.Join(".", path, item.Name).TrimStart('.');

                var itemAttrbs = item.GetAttributes();

                var fromFormAttribute = itemAttrbs.FirstOrDefault(x => x.AttributeClass.Name.Equals(FromFormAttributeFullName));


                var formNameValue = fromFormAttribute?.GetNamedArgumentValue(nameof(FromFormAttribute.Name));

                if (itemAttrbs.Any(x => x.AttributeClass.Name.Equals(FromHeaderAttributeFullName)))
                {
                    continue;
                }
                else if (item.GetTypeSymbol().Name.Equals(IFormFileCollectionFullName))
                {
                    var fakePath = endPath.Replace(".", "__");

                    methodBuilder.AppendLine($"if ({fakePath} != null)");
                    methodBuilder.AppendBodyTabContent(() =>
                    {
                        methodBuilder.AppendLine($"foreach(var ____item in {fakePath})");
                        methodBuilder.AppendBodyTabContent(() =>
                        {
                            methodBuilder.AppendLine($"____content.Add(StreamHttpContent.Create(____item.Stream), \"{formNameValue ?? endPath}\", ____item.FileName);");
                        });
                    });
                }
                else if (item.GetTypeSymbol().Name.Equals(IFormFileFullName))
                {
                    var fakePath = endPath.Replace(".", "__");

                    methodBuilder.AppendLine($"if ({fakePath} != null)");
                    methodBuilder.AppendBodyTabContent(() =>
                    {
                        methodBuilder.AppendLine($"____content.Add(StreamHttpContent.Create({fakePath}.Stream), \"{formNameValue ?? endPath}\", {fakePath}.FileName);");
                    });
                }
                else if (itemAttrbs.Any(x => x.AttributeClass.Name.Equals(ParameterAttributeFullName)))
                {
                    BuildForm(item.GetTypeSymbol().GetAllMembers().Where(x => x is IPropertySymbol).ToImmutableArray(), methodBuilder, item.Name);
                }
                else
                {
                    methodBuilder.AppendLine($"____content.Add(JsonHttpContent.Create({endPath}), \"{formNameValue ?? endPath}\");");
                }
            }
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
        private readonly string ParameterAttributeFullName = typeof(HttpEndPointParameterAttribute).Name;
        private readonly string IFormFileFullName = typeof(IFormFile).Name;
        private readonly string IFormFileCollectionFullName = typeof(IFormFileCollection).Name;
        private readonly string FromHeaderAttributeFullName = typeof(FromHeaderAttribute).Name;
        private readonly string FromBodyAttributeFullName = typeof(FromBodyAttribute).Name;
        private readonly string FromFormAttributeFullName = typeof(FromFormAttribute).Name;

        private readonly string RouteAttributeFullName = "RouteAttribute";
        private readonly string BaseHttpRequestOptionsFullName = "BaseHttpRequestOptions";
    }
}
