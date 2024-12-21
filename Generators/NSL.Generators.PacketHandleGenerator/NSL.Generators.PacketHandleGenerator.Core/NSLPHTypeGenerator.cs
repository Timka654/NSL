﻿//#if DEBUGEXAMPLES
//#define DEVELOP
//#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryGenerator.Utils;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.Generators.PacketHandleGenerator.Utils;
using NSL.Generators.Utils;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace NSL.Generators.PacketHandleGenerator
{
    [Generator]
    internal class NSLPHTypeGenerator : ISourceGenerator
    {
        private void ProcessNSLPHTypes(GeneratorExecutionContext context, NSLPHAttributeSyntaxReceiver methodSyntaxReceiver)
        {
#if DEBUG
            //GenDebug.Break();
#endif

            foreach (var type in methodSyntaxReceiver.BinaryIOTypes)
            {
                try
                {
                    ProcessNSLPHType(context, type);
                }
                catch (Exception ex)
                {
                    context.ShowPHDiagnostics("NSLHP999", $"Error on build {ex.ToString()}", DiagnosticSeverity.Error, type.GetLocation());
                }
            }
        }

        private void ProcessNSLPHType(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            if (!type.HasPartialModifier())
            {
                context.ShowPHDiagnostics("NSLHP000", "Type must have a partial modifier", DiagnosticSeverity.Error, type.GetLocation());
                return;
            }
            var typeClass = type as ClassDeclarationSyntax;

            var typeSem = context.Compilation.GetSemanticModel(typeClass.SyntaxTree);
            var codeBuilder = new CodeBuilder();

            codeBuilder.AppendComment(() =>
            {
                codeBuilder.AppendLine($"Auto Generated by NSL NSLHP. Please don't change this file");
                codeBuilder.AppendLine($"Project must have reference \"NSL.SocketCore\" library for normal working");
            });

            List<string> requiredUsings = new List<string> {
                "NSL.SocketCore",
                "NSL.SocketCore.Utils.Buffer",
                "System.Linq",
                "System"
            };

            var classSymbol = typeSem.GetDeclaredSymbol(typeClass);

            var allClassDeclarations = classSymbol.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(context.CancellationToken))
                .OfType<ClassDeclarationSyntax>();

            var classMethodIdentifiers = allClassDeclarations
                .SelectMany(c => c.Members.OfType<MethodDeclarationSyntax>())
                .Select(x => x.Identifier.Text)
                .ToList();

            codeBuilder.CreatePartialClass(typeClass, classBuilder =>
            {
                var typeAttributes = typeClass.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.GetAttributeFullName().Equals(NSLPHGenImplAttributeFullName)).ToArray();

                var defaultsAttribute = typeClass.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.GetAttributeFullName().Equals(NSLPHGenDefaultsAttributeFullName)).FirstOrDefault();


                var defaultParameters = defaultsAttribute?
                .ArgumentList?
                .Arguments
                .Where(n => n.NameEquals != null)
                .ToDictionary(n => n.NameEquals.Name.ToString(), n => n);
                //GenDebug.Break();

                //var methodNames = type.Members
                //.Where(x => x is MethodDeclarationSyntax)
                //.Cast<MethodDeclarationSyntax>()
                //.Select(x => x.Identifier.Text)
                ////.Where(x => x.StartsWith("Receive") && x.EndsWith("Handle"))
                //.ToArray();

                var typeModels = typeAttributes
                    .Select(x =>
                    {
                        var sem = context.Compilation.GetSemanticModel(x.SyntaxTree);

                        var attributeConstructor = sem.GetSymbolInfo(x).Symbol as IMethodSymbol;

                        var attributeParameters = attributeConstructor.Parameters;

                        var args = x.ArgumentList?.Arguments;

                        var r = new HandlesData()
                        {
                            Context = context,
                            HaveReceiveHandleImplementation = classMethodIdentifiers.Contains
                        };


                        var parameters = args.Value
                        .Where(n => n.NameEquals != null)
                        .ToDictionary(n => n.NameEquals.Name.ToString(), n => n);

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.PacketsEnum), out var packetsType)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.PacketsEnum), out packetsType) == true)
                        {
                            r.Type = packetsType.GetAttributeTypeParameterValueSymbol(typeSem);
                        }
                        else
                            context.ShowPHDiagnostics("NSLHP004", $"Required parameter {"PacketsEnum"}", DiagnosticSeverity.Error, attributeParameters[1].Locations.ToArray());

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.NetworkDataType), out var networkDataType)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.NetworkDataType), out networkDataType) == true)
                            r.NetworkDataType = networkDataType.GetAttributeTypeParameterValueSymbol(typeSem);
                        else
                            context.ShowPHDiagnostics("NSLHP004", $"Required parameter {"NetworkDataType"}", DiagnosticSeverity.Error, attributeParameters[1].Locations.ToArray());

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.Direction), out var direction)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.Direction), out direction) == true)
                            r.Direction = direction.GetAttributeParameterValue<NSLHPDirTypeEnum>(typeSem);

                        r.Modifiers = NSLAccessModifierEnum.Private | NSLAccessModifierEnum.Static;

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.Modifier), out var modifier)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.Modifier), out modifier) == true)
                            r.Modifiers = modifier.GetAttributeParameterValue<NSLAccessModifierEnum>(typeSem);

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.IsStaticNetwork), out var isStaticNetwork)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.IsStaticNetwork), out isStaticNetwork) == true)
                            r.IsStaticNetwork = isStaticNetwork.GetAttributeParameterValue<bool>(typeSem);

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.IsAsync), out var isAsync)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.IsAsync), out isAsync) == true)
                            r.IsAsync = isAsync.GetAttributeParameterValue<bool>(typeSem);

                        if (parameters.TryGetValue(nameof(NSLPHGenImplAttribute.DelegateOutputResponse), out var delegateOutputResponse)
                        || defaultParameters?.TryGetValue(nameof(NSLPHGenImplAttribute.DelegateOutputResponse), out delegateOutputResponse) == true)
                            r.DelegateOutputResponse = delegateOutputResponse.GetAttributeParameterValue<bool>(typeSem);

                        if (Enum.GetValues(typeof(NSLAccessModifierEnum))
                        .Cast<NSLAccessModifierEnum>()
                        .Where(n => n < NSLAccessModifierEnum.Static)
                        .Where(n => r.Modifiers.HasFlag(n))
                            .Count() > 3)
                            context.ShowPHDiagnostics("NSLHP001", "Have invalid modifier combination", DiagnosticSeverity.Error, attributeParameters[1].Locations.ToArray());

                        r.Models = args.Value.Where(n => n.NameEquals == null)
                        .Select(n => n.GetAttributeParameterValue<string>(typeSem))
                        .ToArray();

                        r.Packets = loadPackets(r, context);

                        return r;
                    })
                    //.GroupBy(x => x).Select(x => x.Key)
                    .ToArray();

                if (typeModels.Length > 0)
                {
                    bool haveReceiveRequest = typeModels.Any(x => x.Direction == NSLHPDirTypeEnum.Receive && x.Packets.Any(n => n.PacketType == NSLPacketTypeEnum.Request));

                    if (haveReceiveRequest)
                    {
                        requiredUsings.Add("NSL.SocketCore.Extensions.Buffer");
                    }

                    var cbData = new CodeBuilderData();

                    foreach (var item in typeModels)
                    {
                        var typeCBData = new CodeBuilderData();

                        if (item.Direction == NSLHPDirTypeEnum.Receive)
                            BuildInputType(item, typeSem, typeCBData);
                        else
                            BuildSendType(item, typeSem, typeCBData);

                        cbData.PacketHandlesBuilder.AppendLine(typeCBData.PacketHandlesBuilder.ToString());
                        cbData.HandlesBuilder.AppendLine(typeCBData.HandlesBuilder.ToString());
                        if (item.Direction == NSLHPDirTypeEnum.Receive)
                        {
                            cbData.ConfigureBuilder.AppendLine($"{item.BuildModifierForHandles(NSLAccessModifierEnum.Protected)} void NSLConfigurePacketHandles({nameof(CoreOptions)}<{item.NetworkDataType.GetTypeFullName()}> options)");
                            cbData.ConfigureBuilder.AppendBodyTabContent(() =>
                            {
                                cbData.ConfigureBuilder.AppendLine(typeCBData.ConfigureBuilder.ToString());
                            });
                        }
                    }

                    var tm = typeModels.FirstOrDefault(x => x.IsStaticNetwork && x.Direction == NSLHPDirTypeEnum.Send);

                    if (tm != null)
                    {
                        if (!classMethodIdentifiers.Contains("GetNetworkClient"))
                            classBuilder.AppendLine($"{tm.BuildModifierForHandles(NSLAccessModifierEnum.Protected)} partial {tm.NetworkDataType.GetTypeFullName()} GetNetworkClient();");

                        if (!classMethodIdentifiers.Contains("GetRequestProcessor"))
                            classBuilder.AppendLine($"{tm.BuildModifierForHandles(NSLAccessModifierEnum.Protected)} partial NSL.SocketCore.Extensions.Buffer.RequestProcessor GetRequestProcessor();");
                    }

                    if (cbData.HandlesBuilder.Length > 0)
                    {
                        classBuilder.AppendLine(cbData.HandlesBuilder.ToString());
                        classBuilder.AppendLine();
                    }

                    if (cbData.PacketHandlesBuilder.Length > 0)
                        classBuilder.AppendLine(cbData.PacketHandlesBuilder.ToString());

                    if (cbData.ConfigureBuilder.Length > 0)
                    {
                        classBuilder.AppendLine();
                        classBuilder.AppendLine(cbData.ConfigureBuilder.ToString());
                    }
                }
            }, requiredUsings);

            // Visual studio have lag(or ...) cannot show changes any time
#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            System.IO.File.WriteAllText($@"C:\Temp\{typeClass.GetClassName()}.ph.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
#endif

#if DEBUG
            //GenDebug.Break();
#endif
            context.AddSource($"{typeClass.GetTypeClassName()}.ph.cs", codeBuilder.ToString());
        }

        private void BuildInputType(HandlesData handle, SemanticModel typeSem, CodeBuilderData buildData)
        {
            foreach (var item in handle.Packets)
            {
                BuildReceivePacket(item,
                                 typeSem,
                                 buildData);
            }
        }

        private void BuildSendType(HandlesData handle, SemanticModel typeSem, CodeBuilderData buildData)
        {
            foreach (var item in handle.Packets)
            {
                BuildSendPacket(item, typeSem, buildData);
            }
        }

        private void BuildSendPacket(PacketData packet, SemanticModel typeSem, CodeBuilderData buildData)
        {
            int pi = 0;
            var _args = packet.Parameters.Select(x =>
            {
                ++pi;
                return $"{x.Type.GetTypeFullName()} {(x.Name ?? $"item{pi - 1}")}";
            });


            if (!packet.HandlesData.IsStaticNetwork)
            {
                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Message))
                {
                    _args = Enumerable.Repeat($"{packet.HandlesData.NetworkDataType.GetTypeFullName()} client", 1).Concat(_args).ToArray();
                }
                else if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
                {
                    _args = Enumerable.Repeat($"NSL.SocketCore.Extensions.Buffer.RequestProcessor requestProcessor", 1).Concat(_args).ToArray();


                }
            }

            if (packet.HandlesData.DelegateOutputResponse && packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
            {
                var res = "Action";

                if (packet.Result != null)
                {
                    res += $"<{packet.Result.Type.GetTypeFullName()}>";
                }

                _args = _args.Append($"{res} onResponseHandle");
            }

            var args = string.Join(", ", _args);



            var rType = "void";


            if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
            {
                if (!packet.HandlesData.DelegateOutputResponse)
                {
                    rType = packet.Result?.Type.GetTypeFullName();

                    if (rType != null)
                        rType = $"async Task<{rType}>";
                    else
                        rType = $"async Task";
                }
                //else
                //    rType = "async void";
            }

            //Debugger.Break();

            var partName = $"Send{packet.Name}";

            var phb = buildData.PacketHandlesBuilder;

            phb.AppendSummaryLine($"Generate for <see cref=\"{packet.HandlesData.Type.GetTypeFullName()}.{packet.Name}\"/>");
            phb.AppendLine($"{packet.HandlesData.BuildModifiers()} {rType} {partName}({args})");


            phb.AppendBodyTabContent(() =>
            {
                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
                {
                    phb.AppendLine($"var __outputBuf = RequestPacketBuffer.Create({packet.HandlesData.Type.GetTypeFullName()}.{packet.Name});");
                }
                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Message))
                {
                    phb.AppendLine($"var __outputBuf = {nameof(OutputPacketBuffer)}.Create({packet.HandlesData.Type.GetTypeFullName()}.{packet.Name});");
                }

                phb.AppendLine();
                if (packet.Parameters.Any())
                    phb.AppendLine();

                pi = 0;
                foreach (var item in packet.Parameters)
                {
                    string writeSegment = buildParameterBinaryWriteSegment(packet, item, (item.Name ?? $"item{pi}"));

                    phb.AppendLine($"{writeSegment};");
                    phb.AppendLine();

                    ++pi;
                }

                //if (packet.Parameters.Any())
                //    phb.AppendLine();


                bool haveResult = packet.PacketType.HasFlag(NSLPacketTypeEnum.Request) && packet.Result != null;

                bool returnResult = haveResult && !packet.HandlesData.DelegateOutputResponse;
                bool delegateResult = haveResult && packet.HandlesData.DelegateOutputResponse;

                if (returnResult)
                {
                    phb.AppendLine();
                    phb.AppendLine($"{packet.Result.Type.GetTypeFullName()} ___result = default;");
                    phb.AppendLine();
                    //invokeLine = buildResultBinaryWriteSegment(packet, packet.Result, $"___invokeResult");
                }


                string clientField = string.Empty;

                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
                {
                    clientField = packet.HandlesData.IsStaticNetwork ? $"GetRequestProcessor()" : "requestProcessor";

                    var bodyBuilder = new CodeBuilder();

                    bodyBuilder.AppendLine("__response => ");

                    bodyBuilder.AppendBodyTabContent(() =>
                    {
                        if (returnResult)
                        {
                            bodyBuilder.AppendLine($"___result = {buildResultBinaryReadSegment(packet, packet.Result, packet.Result.Type.GetTypeFullName())};");
                            bodyBuilder.AppendLine();
                        }
                        else if (packet.HandlesData.DelegateOutputResponse)
                        {
                            bodyBuilder.AppendLine($"if(onResponseHandle != null)");
                            bodyBuilder.AppendBodyTabContent(() =>
                            {
                                if (packet.Result != null)
                                    bodyBuilder.AppendLine($"onResponseHandle({buildResultBinaryReadSegment(packet, packet.Result, packet.Result.Type.GetTypeFullName())});");
                                else
                                    bodyBuilder.AppendLine($"onResponseHandle();");
                            });
                        }

                        bodyBuilder.AppendLine();

                        if (packet.HandlesData.DelegateOutputResponse)
                            bodyBuilder.AppendLine($"return true;");
                        else
                            bodyBuilder.AppendLine($"return Task.FromResult(true);");
                    });


                    phb.AppendLine();

                    var sendRequestInvokeName = packet.HandlesData.DelegateOutputResponse ? $"{clientField}.SendRequest" : $"await {clientField}.SendRequestAsync";

                    phb.AppendLine($"{sendRequestInvokeName}(__outputBuf, {bodyBuilder});");
                }
                else if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Message))
                {
                    clientField = packet.HandlesData.IsStaticNetwork ? $"GetNetworkClient()" : "client";

                    phb.AppendLine();

                    phb.AppendLine($"{clientField}.Send(__outputBuf);");
                }

                if (returnResult)
                {
                    phb.AppendLine();
                    phb.AppendLine($"return ___result;");
                }
            });

            phb.AppendLine();
        }

        private void BuildReceivePacket(PacketData packet, SemanticModel typeSem, CodeBuilderData buildData)
        {

            bool isAsync = packet.HandlesData.IsAsync;

            int pi = 0;

            var partName = $"Receive{packet.Name}Handle";

            if (!packet.HandlesData.HaveReceiveHandleImplementation(partName))
            {

                var args = string.Join(", ", Enumerable.Repeat($"{packet.HandlesData.NetworkDataType.GetTypeFullName()} client", 1).Concat(packet.Parameters.Select(x =>
                {
                    ++pi;
                    return $"{x.Type.GetTypeFullName()} {(x.Name ?? $"item{pi - 1}")}";
                })).ToArray());



                var rType = isAsync ? "Task" : "void";

                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request) && packet.Result != null)
                {
                    rType = packet.Result.Type.GetTypeFullName();

                    if (isAsync)
                        rType = $"Task<{rType}>";
                }

                //Debugger.Break();

                var hb = buildData.HandlesBuilder;

                hb.AppendSummaryLine($"Generate for <see cref=\"{packet.HandlesData.Type.GetTypeFullName()}.{packet.Name}\"/>");
                hb.AppendLine($"{packet.HandlesData.BuildModifiers()} partial {rType} {partName}({args});");

                hb.AppendLine();
            }

            var phb = buildData.PacketHandlesBuilder;

            phb.AppendLine($"{packet.HandlesData.BuildModifierForHandles()} {(isAsync ? "async Task" : "void")} NSLPacketHandle_{packet.Name}({packet.HandlesData.NetworkDataType.GetTypeFullName()} client, {nameof(InputPacketBuffer)} data)");


            phb.AppendBodyTabContent(() =>
            {
                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
                {
                    phb.AppendLine("var __response = data.CreateResponse();");
                    phb.AppendLine();
                    if (packet.Parameters.Any())
                        phb.AppendLine();
                }

                pi = 0;
                foreach (var item in packet.Parameters)
                {
                    var tname = item.Type.GetTypeFullName();

                    string readSegment = buildParameterBinaryReadSegment(packet, item, tname);

                    phb.AppendLine($"{tname} {(item.Name ?? $"data{pi}")} = {readSegment};");
                    phb.AppendLine();

                    ++pi;
                }

                //if (packet.Parameters.Any())
                //    phb.AppendLine();

                string invokeLine = $"{(isAsync ? "await " : string.Empty)}{partName}({(string.Join(", ", Enumerable.Repeat("client", 1).Concat(Enumerable.Range(0, packet.Parameters.Length).Select(i => packet.Parameters[i].Name ?? $"data{i}"))))})";

                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request) && packet.Result != null)
                {
                    phb.AppendLine();
                    phb.AppendLine($"var ___invokeResult = {invokeLine};");
                    phb.AppendLine();
                    invokeLine = buildResultBinaryWriteSegment(packet, packet.Result, $"___invokeResult");
                }


                phb.AppendLine($"{invokeLine};");

                if (packet.PacketType.HasFlag(NSLPacketTypeEnum.Request))
                {
                    phb.AppendLine();
                    phb.AppendLine($"client.Send(__response);");
                }
            });

            phb.AppendLine();


            var cb = buildData.ConfigureBuilder;

            if (isAsync)
                cb.AppendLine($"options.{nameof(CoreOptions<INetworkClient>.AddAsyncHandle)}((ushort){packet.HandlesData.Type.GetTypeFullName()}.{packet.Name}, NSLPacketHandle_{packet.Name});");
            else
                cb.AppendLine($"options.{nameof(CoreOptions<INetworkClient>.AddHandle)}((ushort){packet.HandlesData.Type.GetTypeFullName()}.{packet.Name}, NSLPacketHandle_{packet.Name});");
        }


        private string buildParameterBinaryReadSegment(PacketData packet, PacketParamData item, string tname)
        {
            var typeAttributes = item.Type.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(NSLBIOTypeAttributeFullName)).ToArray();

            var typeModels = typeAttributes
                .SelectMany(x =>
                {
                    var models = x.ConstructorArguments.SelectMany(n => n.Values.Select(d => (string)d.Value)).ToArray();

                    if (!models.Any())
                        return Enumerable.Repeat<string>("<!!_NSLBIOFULL_!!>", 1);

                    return models;
                })
                .GroupBy(x => x).Select(x => x.Key)
                .ToArray();

            var model = item.BinaryModel ?? "<!!_NSLBIOFULL_!!>";

            if (typeModels.Contains(model))
            {
                if (item.BinaryModel == null)
                    return $"{tname}.ReadFullFrom(data)";
                else
                    return $"{tname}.Read{item.BinaryModel}From(data)";
            }

            return BinaryReadMethodsGenerator.GetValueReadSegment(item.Type, new BinaryGeneratorContext()
            {
                Context = packet.HandlesData.Context,
                IOPath = "data",
                For = model
            }, "data");
        }


        private string buildParameterBinaryWriteSegment(PacketData packet, PacketParamData item, string tname)
        {
            var typeAttributes = item.Type.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(NSLBIOTypeAttributeFullName)).ToArray();

            var typeModels = typeAttributes
                .SelectMany(x =>
                {
                    var models = x.ConstructorArguments.SelectMany(n => n.Values.Select(d => (string)d.Value)).ToArray();

                    if (!models.Any())
                        return Enumerable.Repeat<string>("<!!_NSLBIOFULL_!!>", 1);

                    return models;
                })
                .GroupBy(x => x).Select(x => x.Key)
                .ToArray();

            var model = item.BinaryModel ?? "<!!_NSLBIOFULL_!!>";

            if (typeModels.Contains(model))
            {
                if (item.BinaryModel == null)
                    return $"{tname}.WriteFullTo(__outputBuf)";
                else
                    return $"{tname}.Write{item.BinaryModel}To(__outputBuf)";
            }

            return BinaryWriteMethodsGenerator.BuildParameterWriter(item.Type, new BinaryGeneratorContext()
            {
                Context = packet.HandlesData.Context,
                IOPath = "__outputBuf",
                For = model
            }, tname);
        }
        private string buildResultBinaryWriteSegment(PacketData packet, PacketResultData item, string tname)
        {
            var typeAttributes = item.Type.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(NSLBIOTypeAttributeFullName)).ToArray();

            var typeModels = typeAttributes
                .SelectMany(x =>
                {
                    var models = x.ConstructorArguments.SelectMany(n => n.Values.Select(d => (string)d.Value)).ToArray();

                    if (!models.Any())
                        return Enumerable.Repeat<string>("<!!_NSLBIOFULL_!!>", 1);

                    return models;
                })
                .GroupBy(x => x).Select(x => x.Key)
                .ToArray();

            var model = item.BinaryModel ?? "<!!_NSLBIOFULL_!!>";

            if (typeModels.Contains(model))
            {
                if (item.BinaryModel == null)
                    return $"{tname}.WriteFullTo(__response)";
                else
                    return $"{tname}.Write{item.BinaryModel}To(__response)";
            }

            return BinaryWriteMethodsGenerator.BuildParameterWriter(item.Type, new BinaryGeneratorContext()
            {
                Context = packet.HandlesData.Context,
                IOPath = "__response",
                For = model
            }, tname).TrimEnd(';');
        }
        private string buildResultBinaryReadSegment(PacketData packet, PacketResultData item, string tname)
        {
            var typeAttributes = item.Type.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(NSLBIOTypeAttributeFullName)).ToArray();

            var typeModels = typeAttributes
                .SelectMany(x =>
                {
                    var models = x.ConstructorArguments.SelectMany(n => n.Values.Select(d => (string)d.Value)).ToArray();

                    if (!models.Any())
                        return Enumerable.Repeat<string>("<!!_NSLBIOFULL_!!>", 1);

                    return models;
                })
                .GroupBy(x => x).Select(x => x.Key)
                .ToArray();

            var model = item.BinaryModel ?? "<!!_NSLBIOFULL_!!>";

            if (typeModels.Contains(model))
            {
                if (item.BinaryModel == null)
                    return $"{tname}.ReadFullFrom(__response)";
                else
                    return $"{tname}.Read{item.BinaryModel}From(__response)";
            }

            return BinaryReadMethodsGenerator.GetValueReadSegment(item.Type, new BinaryGeneratorContext()
            {
                Context = packet.HandlesData.Context,
                IOPath = "__response",
                For = model
            }, tname).TrimEnd(';');
        }

        private PacketData[] loadPackets(HandlesData item, GeneratorExecutionContext context)
            => item.Type.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Where(field => field.IsStatic && field.HasConstantValue)
                        .Select(packet =>
                        {
                            var attributes = packet.GetAttributes();

                            var attr = attributes
                            .FirstOrDefault(a => a.AttributeClass.Name == NSLPHGenAttributeFullName);

                            if (attr == null)
                                return null;

                            var args = attr.ConstructorArguments;

                            var r = new PacketData()
                            {
                                HandlesData = item,
                                PacketType = NSLPacketTypeEnum.Message,
                                Name = packet.Name,
                                EnumMember = packet
                            };

                            if (args[0].Type.ToString() == NSLPHGenPacketTypeEnumFullName)
                            {
                                r.PacketType = (NSLPacketTypeEnum)args[0].Value;


                                var modCount = Enum.GetValues(typeof(NSLPacketTypeEnum))
                                .Cast<NSLPacketTypeEnum>()
                                .Where(x => r.PacketType.HasFlag(x))
                                .Count();

                                if (modCount > 1)
                                    context.ShowPHDiagnostics("NSLHP002", "Have invalid packet type", DiagnosticSeverity.Error, packet.Locations.ToArray());

                                if (modCount == 0)
                                {
                                    if (r.Name.EndsWith("Request"))
                                        r.PacketType |= NSLPacketTypeEnum.Request;
                                    else if (r.Name.EndsWith("Message"))
                                        r.PacketType |= NSLPacketTypeEnum.Message;
                                    else
                                        context.ShowPHDiagnostics("NSLHP003", "Need to set packet type (Cannot detect Receive or Message)", DiagnosticSeverity.Error, packet.Locations.ToArray());
                                }

                                r.Models = args
                                .Skip(1)
                                .SelectMany(n => n.Values.Select(v => (string)v.Value))
                                .ToArray();
                            }
                            else
                            {
                                if (r.Name.EndsWith("Request"))
                                    r.PacketType = NSLPacketTypeEnum.Request;
                                else if (r.Name.EndsWith("Message"))
                                    r.PacketType = NSLPacketTypeEnum.Message;

                                //r.PacketType |= PacketTypeEnum.Async;

                                r.Models = args
                                .Skip(0)
                                .SelectMany(n => n.Values.Select(v => (string)v.Value))
                                .ToArray();
                            }


                            if (r.HandlesData.Models.Any() && !r.Models.Any(d => r.HandlesData.Models.Contains(d)))
                            {

                                return null;
                            }

                            loadPacket(r, attributes);

                            return r;
                        })
                        .Where(x => x != null)
                        .ToArray();

        private void loadPacket(PacketData item, System.Collections.Immutable.ImmutableArray<AttributeData> attributes)
        {
            var parameters = attributes
            .Where(a => a.AttributeClass.Name == NSLPHGenParamAttributeFullName);

            item.Parameters = parameters.Select(x =>
            {
                var r = new PacketParamData()
                {
                    Type = (ITypeSymbol)x.ConstructorArguments[0].Value,
                    Attribute = x
                };

                if (x.ConstructorArguments.Length > 1)
                    r.BinaryModel = (string)x.ConstructorArguments[1].Value;

                var name = x.NamedArguments.FirstOrDefault(n => n.Key == "Name");

                if (!Equals(name, default))
                    r.Name = (string)name.Value.Value;

                return r;
            }).ToArray();



            if (item.PacketType.HasFlag(NSLPacketTypeEnum.Request))
            {
                var result = attributes
                .FirstOrDefault(a => a.AttributeClass.Name == NSLPHGenResultAttributeFullName);

                if (result != null)
                {
                    item.Result = new PacketResultData()
                    {
                        Type = (ITypeSymbol)result.ConstructorArguments[0].Value,
                        BinaryModel = result.ConstructorArguments.Length > 1 ? (string)result.ConstructorArguments[1].Value : default
                    };
                }
            }
        }

        #region ISourceGenerator

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is NSLPHAttributeSyntaxReceiver methodSyntaxReceiver)
            {
                ProcessNSLPHTypes(context, methodSyntaxReceiver);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new NSLPHAttributeSyntaxReceiver());
        }

        #endregion

        internal static readonly string NSLPHGenImplAttributeFullName = typeof(NSLPHGenImplAttribute).Name;

        internal static readonly string NSLPHGenDefaultsAttributeFullName = typeof(NSLPHGenImplDefaultsAttribute).Name;

        internal static readonly string NSLPHGenAttributeFullName = typeof(NSLPHGenAttribute).Name;

        internal static readonly string NSLPHGenParamAttributeFullName = typeof(NSLPHGenArgAttribute).Name;

        internal static readonly string NSLPHGenResultAttributeFullName = typeof(NSLPHGenResultAttribute).Name;

        internal static readonly string NSLPHGenAccessModifierEnumFullName = typeof(NSLAccessModifierEnum).FullName;

        internal static readonly string NSLPHGenPacketTypeEnumFullName = typeof(NSLPacketTypeEnum).FullName;

        internal static readonly string NSLBIOTypeAttributeFullName = typeof(NSLBIOTypeAttribute).Name;
    }
}
