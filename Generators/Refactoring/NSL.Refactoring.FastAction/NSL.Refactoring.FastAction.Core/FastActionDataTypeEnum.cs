using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace NSL.Refactoring.FastAction.Core
{
    internal enum FastActionDataTypeEnum : byte
    {
        MemberType,
        PropertyMemberType,
        FieldMemberType,
        Type,
        EnumType,
        ParameterType,

        BaseNodeName,
        TypeName,
        NamespaceName,
        EnumName,
        AttributeName,
        NodeName,

        Property,
        Field,
        EnumMember,
        Parameter,
        Attribute,
        Method,
        Enum,
        Class,
        Struct,
        Namespace,
        Node,
        Member,
        BaseType
    }

    internal class FASessionData : Dictionary<FastActionDataTypeEnum, object>
    {
        public FASessionData((FastActionDataTypeEnum, object)[] items)
        {
            foreach (var item in items)
            {
                if (item.Item2 == default) continue;

                Add(item.Item1, item.Item2);
            }
        }

        bool validateInput(ConditionData condition, string input, string[] match, Dictionary<string, string> matches)
        {
            Dictionary<string, string> nnmatch = null;

            if (input == default || !condition.IsMatch(match, input, out nnmatch))
            {
                if (!condition.OptionalCondition)
                    return false;
            }

            if (nnmatch != null)
                foreach (var item in nnmatch)
                {
                    matches[item.Key] = item.Value;
                }

            return true;
        }

        bool validateNamespace(ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.Namespace != default)
            {
                if (!TryGetValue(FastActionDataTypeEnum.Attribute, out var attribute))
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var ns = ((AttributeSyntax)attribute).Name.ToString();

                return validateInput(condition, ns, condition.Namespace, matches);
            }

            return true;
        }

        bool validateProperty(ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.PropertyDeclaration != default)
            {
                if (!TryGetValue(FastActionDataTypeEnum.Property, out var attribute))
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var ns = ((PropertyDeclarationSyntax)attribute).ToString();

                return validateInput(condition, ns, condition.PropertyDeclaration, matches);
            }
            return true;
        }

        bool validateMemberType(ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.MemberType != default)
            {
                if (!TryGetValue(FastActionDataTypeEnum.MemberType, out var attribute))
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var ns = ((PropertyDeclarationSyntax)attribute).ToString();

                return validateInput(condition, ns, condition.MemberType, matches);
            }
            return true;
        }

        bool validateField(ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.FieldDeclaration != default)
            {
                if (!TryGetValue(FastActionDataTypeEnum.Field, out var attribute))
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var ns = ((FieldDeclarationSyntax)attribute).ToString();

                return validateInput(condition, ns, condition.FieldDeclaration, matches);
            }

            return true;
        }

        bool validateFilePath(Document document, ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.FilePath != default)
            {
                if (document.FilePath == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                return validateInput(condition, document.FilePath, condition.FilePath, matches);
            }

            return true;
        }

        bool validateNodeName(ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.NodeName != default)
            {
                if (!TryGetValue(FastActionDataTypeEnum.NodeName, out var attribute))
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var ns = (string)attribute;

                return validateInput(condition, ns, condition.NodeName, matches);
            }

            return true;
        }

        bool validateProjectPath(Document document, ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.ProjectPath != default)
            {
                if (document.Project.FilePath == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                return validateInput(condition, document.Project.FilePath, condition.ProjectPath, matches);
            }

            return true;
        }

        bool validateProjectName(Document document, ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.ProjectName != default)
            {
                if (document.Project.Name == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                return validateInput(condition, document.Project.Name, condition.ProjectName, matches);
            }

            return true;
        }

        bool validateSolutionPath(Document document, ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.SolutionPath != default)
            {
                if (document.Project.Solution.FilePath == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                return validateInput(condition, document.Project.Solution.FilePath, condition.SolutionPath, matches);
            }

            return true;
        }

        bool validateSolutionName(Document document, ConditionData condition, Dictionary<string, string> matches)
        {
            if (condition.SolutionName != default)
            {
                if (document.Project?.Solution?.FilePath == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                var name = Path.GetFileName(document.Project.Solution.FilePath);

                name = name.Substring(0, name.LastIndexOf('.'));

                if (name == default)
                {
                    if (condition.OptionalExists)
                        return true;

                    return false;
                }

                return validateInput(condition, name, condition.SolutionName, matches);
            }

            return true;
        }

        public bool ValidateConditions(Document document, ActionData action, out Dictionary<string, string> matches)
        {
            matches = new Dictionary<string, string>();

            foreach (var condition in action.Conditions)
            {
                if (!validateNamespace(condition, matches)) return false;
                if (!validateProperty(condition, matches)) return false;
                if (!validateField(condition, matches)) return false;
                if (!validateMemberType(condition, matches)) return false;
                if (!validateFilePath(document, condition, matches)) return false;
                if (!validateNodeName(condition, matches)) return false;
                if (!validateProjectPath(document, condition, matches)) return false;
                if (!validateProjectName(document, condition, matches)) return false;
                if (!validateSolutionPath(document, condition, matches)) return false;
                if (!validateSolutionName(document, condition, matches)) return false;
            }

            return true;
        }

        public bool ValidateTypeCondition(ActionData action)
        {


            var typeAllow = action.Types == null || action.Types.Contains("*");


            typeAllow = typeAllow || (action.Types.Contains("namespace_decl") && ContainsKey(FastActionDataTypeEnum.Namespace));

            typeAllow = typeAllow || (action.Types.Contains("type_decl") && ContainsKey(FastActionDataTypeEnum.Type));

            typeAllow = typeAllow || (action.Types.Contains("class_decl") && ContainsKey(FastActionDataTypeEnum.Class));

            typeAllow = typeAllow || (action.Types.Contains("struct_decl") && ContainsKey(FastActionDataTypeEnum.Struct));

            typeAllow = typeAllow || (action.Types.Contains("method_decl") && ContainsKey(FastActionDataTypeEnum.Method));


            typeAllow = typeAllow || (action.Types.Contains("enum_member") && ContainsKey(FastActionDataTypeEnum.EnumMember));

            typeAllow = typeAllow || (action.Types.Contains("enum_decl") && ContainsKey(FastActionDataTypeEnum.Enum));

            typeAllow = typeAllow || (action.Types.Contains("member_decl") && ContainsKey(FastActionDataTypeEnum.Member));

            typeAllow = typeAllow || (action.Types.Contains("property_decl") && ContainsKey(FastActionDataTypeEnum.Property));

            typeAllow = typeAllow || (action.Types.Contains("field_decl") && ContainsKey(FastActionDataTypeEnum.Field));

            typeAllow = typeAllow || (action.Types.Contains("attribute") && ContainsKey(FastActionDataTypeEnum.Attribute));

            typeAllow = typeAllow || (action.Types.Contains("parameter") && ContainsKey(FastActionDataTypeEnum.Parameter));

            typeAllow = typeAllow || (action.Types.Contains("base_type") && ContainsKey(FastActionDataTypeEnum.BaseType));

            return typeAllow;
        }
    }
}
