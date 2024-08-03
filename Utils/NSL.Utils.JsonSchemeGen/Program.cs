using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NJsonSchema;
using NSL.Generators.Utils;
using NSL.Utils.JsonSchemeGen.Attributes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NSL.Utils.JsonSchemeGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var v = new ClassDeclarationVisitor();

            List<SyntaxTree> trees = new List<SyntaxTree>();

            foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cs", SearchOption.AllDirectories))
            {
                var fileCode = File.ReadAllText(item);

                var tree = CSharpSyntaxTree.ParseText(fileCode);

                trees.Add(tree);

                var rootNode = tree.GetRoot();

                v.Visit(rootNode);
            }


            // Create a compilation for the syntax tree
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: trees,
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

            // Get the semantic model


            JsonSchema jsonSchema = new JsonSchema();


            foreach (var type in v.ClassDeclarations)
            {
                var semanticModel = compilation.GetSemanticModel(type.Declaration.SyntaxTree);

                var name = type.GetName(semanticModel);

                foreach (var property in type.Properties)
                {
                    var pName = property.SchemeArgs.FirstOrDefault(x => x.GetName() == nameof(NSLJsonSchemePropertyAttribute.Name))?.GetAttributeParameterValue<string>(semanticModel);

                    pName ??= property.Declaration.Identifier.Text;

                    string? pDescription = property.SchemeArgs.FirstOrDefault(x => x.GetName() == nameof(NSLJsonSchemePropertyAttribute.Description))?.GetAttributeParameterValue<string>(semanticModel);

                    jsonSchema.Properties.Add(pName, new JsonSchemaProperty() { Description = pDescription });
                }
            }

            var json = jsonSchema.ToJson();
            File.WriteAllText("dev.json", json);
        }

        public class GenTypeContainer(ClassDeclarationSyntax declaration, AttributeSyntax schemeAttribute)
        {
            public ClassDeclarationSyntax Declaration { get; } = declaration;

            public AttributeSyntax SchemeAttribute { get; } = schemeAttribute;

            public List<GenPropertyContainer> Properties { get; } = new List<GenPropertyContainer>();

            public IEnumerable<AttributeArgumentSyntax> SchemeArgs => SchemeAttribute.ArgumentList.Arguments;

            public string GetName(SemanticModel semantic) => SchemeArgs.First().GetAttributeParameterValue<string>(semantic);
        }

        public class GenPropertyContainer(PropertyDeclarationSyntax declaration, AttributeSyntax schemeAttribute)
        {
            public PropertyDeclarationSyntax Declaration { get; } = declaration;

            public AttributeSyntax SchemeAttribute { get; } = schemeAttribute;

            public IEnumerable<AttributeArgumentSyntax> SchemeArgs => SchemeAttribute.ArgumentList.Arguments;
        }



        class ClassDeclarationVisitor : CSharpSyntaxWalker
        {

            public List<GenTypeContainer> ClassDeclarations { get; } = new();

            private GenTypeContainer? currentContainer = null;

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // Add the class declaration to the list
                //if (node.Identifier.Text.Contains("JWTIdentityDataModel"))
                //{
                //    var t = (node.AttributeLists.First().Attributes.First().Name as IdentifierNameSyntax).Identifier;
                //    Debugger.Break();
                //}
                var attributeList = node.AttributeLists.SelectMany(x => x.Attributes);

                var genAttribute = attributeList.FirstOrDefault(b => string.Equals(b.Name.GetAttributeFullName(), (nameof(NSLJsonSchemeAttribute))));


                if (genAttribute != null)
                {
                    currentContainer = new GenTypeContainer(node, genAttribute);
                    ClassDeclarations.Add(currentContainer);
                }

                // Continue visiting the children nodes
                base.VisitClassDeclaration(node);

                currentContainer = null;
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (currentContainer != null)
                {
                    var attributeList = node.AttributeLists.SelectMany(x => x.Attributes);

                    var genAttribute = attributeList.FirstOrDefault(b => string.Equals(b.Name.GetAttributeFullName(), (nameof(NSLJsonSchemePropertyAttribute))));


                    if (genAttribute != null)
                    {
                        currentContainer.Properties.Add(new GenPropertyContainer(node, genAttribute));
                    }
                }
                base.VisitPropertyDeclaration(node);
            }
        }
    }
}
