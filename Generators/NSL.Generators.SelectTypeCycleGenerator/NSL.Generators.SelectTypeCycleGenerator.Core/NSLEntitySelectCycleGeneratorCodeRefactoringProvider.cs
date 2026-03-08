using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NSL.Entity.SelectCycleGenerator.Shared;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Entity.SelectCycleGenerator
{
    [Generator(LanguageNames.CSharp)]
    internal class NSLEntitySelectCycleGeneratorCodeRefactoringProvider : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 1. Ищем классы с атрибутом SqlCycleSelect
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => IsTargetForGeneration(s),
                    transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(m => m != null); // Отсекаем null

            // 2. Комбинируем с компиляцией для доступа к семантической модели
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            // 3. Передаем в генерацию
            context.RegisterSourceOutput(compilationAndClasses,
                (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static bool IsTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0;
        }

        private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Быстрая синтаксическая проверка
            foreach (var attributeListSyntax in classDeclaration.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (attributeSyntax.Name.ToString().Contains("SqlCycleSelect"))
                    {
                        return classDeclaration;
                    }
                }
            }
            return null;
        }
        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty) return;

            foreach (var classSyntax in classes.Distinct())
            {
                var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

                if (classSymbol == null) continue;

                var metadata = ParseMetadata(classSymbol);
                if (metadata == null || metadata.SelectModels.Count == 0) continue;

                string generatedCode = GenerateExtensionClass(metadata);
                context.AddSource($"{classSymbol.Name}_SqlCycleExtensions.g.cs", generatedCode);
            }
        }
        private static CycleClassMetadata ParseMetadata(INamedTypeSymbol classSymbol)
        {
            var metadata = new CycleClassMetadata
            {
                ClassName = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace.ToDisplayString()
            };

            // ---- ОПРЕДЕЛЯЕМ ИМЯ ТАБЛИЦЫ ----
            var tableAttr = classSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "TableAttribute" || a.AttributeClass?.Name == "Table");
            if (tableAttr != null && tableAttr.ConstructorArguments.Length > 0)
            {
                metadata.TableName = tableAttr.ConstructorArguments[0].Value?.ToString();
            }
            else
            {
                // По умолчанию отрезаем Model/Entity и добавляем 's'
                string name = classSymbol.Name;
                if (name.EndsWith("Model")) name = name.Substring(0, name.Length - 5);
                else if (name.EndsWith("Entity")) name = name.Substring(0, name.Length - 6);
                metadata.TableName = name + "s";
            }

            // 1. Разбираем атрибуты класса (без изменений)
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrName = attr.AttributeClass?.Name;

                if (attrName == "SqlCycleSelectAttribute" || attrName == "SqlCycleSelect")
                {
                    metadata.SelectModels.AddRange(ExtractStringArray(attr.ConstructorArguments.FirstOrDefault()));
                }
                else if (attrName == "SqlCycleHierarchyAttribute" || attrName == "SqlCycleHierarchy")
                {
                    metadata.ParentProperty = new CyclePropertyMetadata
                    {
                        RelationColumn = attr.ConstructorArguments[0].Value?.ToString() ?? "ParentId"
                    };
                }
                else if (attrName == "SqlCyclePaginationAttribute" || attrName == "SqlCyclePagination")
                {
                    var meta = new CyclePaginationMetadata();

                    if (attr.ConstructorArguments.Length >= 2)
                    {
                        meta.Skip = (bool)(attr.ConstructorArguments[0].Value ?? false);
                        meta.Take = (bool)(attr.ConstructorArguments[1].Value ?? false);

                        if (attr.ConstructorArguments.Length >= 3)
                        {
                            meta.Models = ExtractStringArray(attr.ConstructorArguments[2]);
                        }
                    }

                    // Читаем Target из именованных аргументов (если он задан)
                    meta.Target = 1; // Default: Anchor
                    var targetArg = attr.NamedArguments.FirstOrDefault(na => na.Key == "Target");
                    if (targetArg.Key != null && targetArg.Value.Value != null)
                    {
                        meta.Target = (int)targetArg.Value.Value;
                    }

                    metadata.Paginations.Add(meta);
                }
            }

            // 2. Разбираем свойства с учетом БАЗОВЫХ КЛАССОВ
            var currentSymbol = classSymbol;
            var handledProperties = new HashSet<string>(); // Чтобы не дублировать переопределенные свойства (override)

            while (currentSymbol != null && currentSymbol.SpecialType != SpecialType.System_Object)
            {
                foreach (var member in currentSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    if (!handledProperties.Add(member.Name)) continue; // Пропускаем, если свойство уже обработано

                    //if (!Debugger.IsAttached)
                    //    Debugger.Launch();
                    //else
                    //    Debugger.Break();
                    foreach (var attr in member.GetAttributes())
                    {
                        var attrName = attr.AttributeClass?.Name;
                        if (attrName == "SelectGenerateIncludeAttribute" || attrName == "SelectGenerateInclude")
                        {
                            metadata.Members.Add(new CycleMemberMetadata
                            {
                                PropertyName = member.Name,
                                Models = ExtractStringArray(attr.ConstructorArguments.FirstOrDefault())
                            });
                        }
                        else if (attrName == "SelectGenerateProxyAttribute" || attrName == "SelectGenerateProxy")
                        {
                            var proxy = new CycleProxyMetadata { PropertyName = member.Name };

                            if (attr.ConstructorArguments.Length == 1)
                            {
                                proxy.ToModel = attr.ConstructorArguments[0].Value?.ToString();
                                proxy.FromModels = new string[0];
                            }
                            else if (attr.ConstructorArguments.Length == 2)
                            {
                                var fromModel = attr.ConstructorArguments[0].Value?.ToString();
                                proxy.ToModel = attr.ConstructorArguments[1].Value?.ToString();
                                proxy.FromModels = string.IsNullOrEmpty(fromModel) ? new string[0] : new[] { fromModel };
                            }

                            metadata.Proxies.Add(proxy);
                        }
                        else if (attrName == "SqlCycleFilterAttribute" || attrName == "SqlCycleFilter")
                        {
                            // Убрали добавление "?", берем оригинальный тип свойства
                            string propType = member.Type.ToDisplayString();

                            int targetValue = 1; // Both

                            var targetArg = attr.NamedArguments.FirstOrDefault(na => na.Key == "Target");
                            if (targetArg.Key != null && targetArg.Value.Value != null)
                            {
                                targetValue = (int)targetArg.Value.Value;
                            }

                            metadata.Filters.Add(new CycleFilterMetadata
                            {
                                PropertyName = member.Name,
                                PropertyType = propType,
                                Models = ExtractStringArray(attr.ConstructorArguments.FirstOrDefault()),
                                Target = targetValue
                            });
                        }
                        else if (attrName == "SqlCycleOrderByAttribute" || attrName == "SqlCycleOrderBy")
                        {
                            var orderByMeta = new CycleOrderByMetadata
                            {
                                PropertyName = member.Name,
                                Models = ExtractStringArray(attr.ConstructorArguments.FirstOrDefault()),
                                Descending = true,
                                Priority = 0,
                                Target = 0 // Both
                            };

                            foreach (var namedArg in attr.NamedArguments)
                            {
                                if (namedArg.Key == "Descending" && namedArg.Value.Value != null)
                                    orderByMeta.Descending = (bool)namedArg.Value.Value;
                                else if (namedArg.Key == "Priority" && namedArg.Value.Value != null)
                                    orderByMeta.Priority = (int)namedArg.Value.Value;
                                else if (namedArg.Key == "Target" && namedArg.Value.Value != null)
                                    orderByMeta.Target = (int)namedArg.Value.Value;
                            }

                            metadata.OrderBys.Add(orderByMeta);
                        }
                    }
                }
                // Поднимаемся на уровень выше по иерархии наследования
                currentSymbol = currentSymbol.BaseType;
            }

            return metadata;
        }

        // Вспомогательный метод для извлечения массивов из аргументов атрибута
        private static string[] ExtractStringArray(TypedConstant constant)
        {
            if (constant.Kind == TypedConstantKind.Array && !constant.IsNull)
            {
                var list = new List<string>();
                foreach (var val in constant.Values)
                {
                    if (val.Value != null)
                    {
                        list.Add(val.Value.ToString());
                    }
                }
                return list.ToArray();
            }
            return new string[0];
        }
        private static string GenerateExtensionClass(CycleClassMetadata metadata)
        {
            var sb = new StringBuilder();

            // 1. Заголовки и usings
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine();

            // 2. Namespace (C# 7.3 style)
            sb.AppendLine($"namespace {metadata.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {metadata.ClassName}SqlCycleExtensions");
            sb.AppendLine("    {");

            // 3. Генерируем методы под каждую запрошенную модель
            foreach (var model in metadata.SelectModels)
            {
                GenerateMethodForModel(sb, metadata, model);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
        private static void GenerateMethodForModel(StringBuilder sb, CycleClassMetadata metadata, string modelName)
        {
            var proxyNames = metadata.Proxies.Select(p => p.PropertyName).GroupBy(x => x).Select(x => x.Key).ToArray();

            var members = metadata.Members
                .Where(m => (m.Models == null || m.Models.Length == 0 || m.Models.Contains(modelName))
                         && !proxyNames.Contains(m.PropertyName))
                .ToList();

            var filters = metadata.Filters.Where(f => f.Models == null || f.Models.Length == 0 || f.Models.Contains(modelName)).ToList();

            // Теперь берем ВСЕ настройки пагинации, а не только первую
            var paginations = metadata.Paginations.Where(p => p.Models == null || p.Models.Length == 0 || p.Models.Contains(modelName)).ToList();

            string idCol = "\"\"Id\"\"";
            string parentIdCol = $"\"\"{metadata.ParentProperty?.RelationColumn ?? "ParentId"}\"\"";
            string tableName = $"\"\"{metadata.TableName}\"\"";

            var selectParts = new List<string>();
            var childSelectParts = new List<string>();

            if (!members.Any(m => m.PropertyName == "Id"))
            {
                selectParts.Add($"c.{idCol}");
                childSelectParts.Add($"child.{idCol}");
            }

            foreach (var m in members)
            {
                selectParts.Add($"c.\"\"{m.PropertyName}\"\"");
                childSelectParts.Add($"child.\"\"{m.PropertyName}\"\"");
            }

            var methodParams = new List<string>();
            var sqlRawArgs = new List<string>();

            var anchorConditions = new StringBuilder();
            var childConditions = new StringBuilder();

            // ====== СТРОГАЯ ЛОГИКА ФИЛЬТРОВ ======
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                string paramName = filter.PropertyName.Substring(0, 1).ToLower() + filter.PropertyName.Substring(1);

                // Добавляем параметр строго, без = default
                methodParams.Add($"{filter.PropertyType} {paramName}");

                // Передаем напрямую, без проверки на null
                sqlRawArgs.Add($"{paramName}");

                // Строгое SQL условие
                if (filter.Target == 0 || filter.Target == 1)
                {
                    anchorConditions.Append($"\r\n        AND c.\"\"{filter.PropertyName}\"\" = @p{i}");
                }

                if (filter.Target == 0 || filter.Target == 2)
                {
                    childConditions.Append($"\r\n        AND child.\"\"{filter.PropertyName}\"\" = @p{i}");
                }
            }

            // ====== ЛОГИКА ПАГИНАЦИИ ======
            string limitOffsetAnchor = "";
            string limitOffsetRecursion = "";

            foreach (var pagination in paginations)
            {
                // Формируем префикс для параметров (пустой для Both)
                string prefix = pagination.Target == 1 ? "anchor" : (pagination.Target == 2 ? "recursion" : "");

                string currentLimit = "";
                string currentOffset = "";

                if (pagination.Take)
                {
                    string takeParamName = prefix == "" ? "take" : prefix + "Take";
                    methodParams.Add($"int {takeParamName}");
                    sqlRawArgs.Add(takeParamName);
                    currentLimit = $"\r\n       LIMIT @p{sqlRawArgs.Count - 1}";
                }

                if (pagination.Skip)
                {
                    string skipParamName = prefix == "" ? "skip" : prefix + "Skip";
                    methodParams.Add($"int {skipParamName}");
                    sqlRawArgs.Add(skipParamName);
                    currentOffset = $"\r\n       OFFSET @p{sqlRawArgs.Count - 1}";
                }

                string clause = $"{currentLimit}{currentOffset}";

                if (pagination.Target == 0 || pagination.Target == 1)
                    limitOffsetAnchor = clause;

                if (pagination.Target == 0 || pagination.Target == 2)
                    limitOffsetRecursion = clause;
            }

            var orderBys = metadata.OrderBys
                .Where(o => o.Models == null || o.Models.Length == 0 || o.Models.Contains(modelName))
                .OrderBy(o => o.Priority) // Сортируем по заданному приоритету
                .ToList();

            string orderByAnchor = "";
            string orderByRecursion = "";

            var anchorOrders = orderBys.Where(o => o.Target == 0 || o.Target == 1).ToList();
            if (anchorOrders.Count > 0)
            {
                var orderParts = anchorOrders.Select(o => $"c.\"\"{o.PropertyName}\"\" {(o.Descending ? "DESC" : "ASC")}");
                orderByAnchor = "\r\n        ORDER BY " + string.Join(", ", orderParts);
            }

            var recursionOrders = orderBys.Where(o => o.Target == 0 || o.Target == 2).ToList();
            if (recursionOrders.Count > 0)
            {
                var orderParts = recursionOrders.Select(o => $"child.\"\"{o.PropertyName}\"\" {(o.Descending ? "DESC" : "ASC")}");
                orderByRecursion = "\r\n        ORDER BY " + string.Join(", ", orderParts);
            }

            string methodParamsString = methodParams.Count > 0 ? $"this DbSet<{metadata.ClassName}> dbSet, " + string.Join(", ", methodParams) : $"this DbSet<{metadata.ClassName}> dbSet";
            string sqlRawArgsString = sqlRawArgs.Count > 0 ? ", " + string.Join(", ", sqlRawArgs) : "";

            string selectClause = string.Join(", ", selectParts);
            string childSelectClause = string.Join(", ", childSelectParts);

            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Генерирует рекурсивный CTE запрос для модели {modelName}.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        public static IQueryable<{metadata.ClassName}> SelectCycle{modelName}({methodParamsString})");
            sb.AppendLine("        {");

            sb.AppendLine(@"            string sql = @""");
            sb.AppendLine($@"WITH RECURSIVE cte AS (
    -- Root
    (
        SELECT 
            {selectClause}
        FROM {tableName} c
        WHERE c.{parentIdCol} IS NULL{anchorConditions}{orderByAnchor}{limitOffsetAnchor}
    )

    UNION ALL

    -- Recursion
    (
        SELECT 
            {childSelectClause}
        FROM {tableName} child
        INNER JOIN cte ON child.{parentIdCol} = cte.{idCol}
        WHERE 1=1{childConditions}{orderByRecursion}{limitOffsetRecursion}
    )
)
SELECT * FROM cte"";");

            sb.AppendLine();
            sb.AppendLine($"            return dbSet.FromSqlRaw(sql{sqlRawArgsString});");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }

    internal class CycleClassMetadata
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string TableName { get; set; }
        public List<string> SelectModels { get; set; } = new List<string>();
        public CyclePropertyMetadata ParentProperty { get; set; }
        public CyclePropertyMetadata ChildsProperty { get; set; }
        public List<CycleMemberMetadata> Members { get; set; } = new List<CycleMemberMetadata>();
        public List<CycleProxyMetadata> Proxies { get; set; } = new List<CycleProxyMetadata>();
        public List<CycleFilterMetadata> Filters { get; set; } = new List<CycleFilterMetadata>(); 
        public List<CyclePaginationMetadata> Paginations { get; set; } = new List<CyclePaginationMetadata>(); public List<CycleOrderByMetadata> OrderBys { get; set; } = new List<CycleOrderByMetadata>();
    }

    internal class CyclePropertyMetadata
    {
        public string PropertyName { get; set; } // Имя свойства в классе
        public string RelationColumn { get; set; } // Колонка связи (например, ParentId)
        public string[] Models { get; set; }
    }

    internal class CycleMemberMetadata
    {
        public string PropertyName { get; set; }
        public string[] Models { get; set; }
    }

    internal class CycleProxyMetadata
    {
        public string PropertyName { get; set; }
        public string ToModel { get; set; }
        public string[] FromModels { get; set; }
    }

    internal class CycleFilterMetadata
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; } // Тип свойства для сигнатуры метода
        public string[] Models { get; set; }
        public int Target { get; set; } = 1; // 0 = Both, 1 = Anchor, 2 = Recursion
    }
    internal class CyclePaginationMetadata
    {
        public bool Skip { get; set; }
        public bool Take { get; set; }
        public int Target { get; set; } = 1; // 0 = Both, 1 = Anchor, 2 = Recursion
        public string[] Models { get; set; }
    }
    internal class CycleOrderByMetadata
    {
        public string PropertyName { get; set; }
        public string[] Models { get; set; }
        public bool Descending { get; set; }
        public int Priority { get; set; }
        public int Target { get; set; }
    }
}
