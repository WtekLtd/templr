using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TemplR.Generator
{
    [Generator]
    public class TemplateGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "TemplR.TemplateAttribute",
                predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
                transform: static (context, CancellationToken) => 
                {
                    var templateClass = context.TargetSymbol;
                    var targetTypeSymbol = (INamedTypeSymbol)context.Attributes[0].ConstructorArguments[0].Value!;
                    return new TemplateModel(
                        templateClass.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                        templateClass.Name,
                        targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        GetPropertyModels(targetTypeSymbol)
                    );
                }
            );

            context.RegisterSourceOutput(pipeline, static (context, model) => 
            {
                using var textWriter = new StringWriter();
                using var codeWriter = new IndentedTextWriter(textWriter);

                codeWriter.WriteLine("using TemplR;");
                codeWriter.WriteLine();

                codeWriter.WriteLine($"namespace {model.Namespace}");
                codeWriter.WriteLine("{");
                codeWriter.Indent++;
                codeWriter.WriteLine($"partial class {model.ClassName}: Template<{model.TargetClassName}>");
                codeWriter.WriteLine("{");
                codeWriter.Indent++;

                foreach (var property in model.Properties)
                {
                    var publicName = property.Name;
                    var privateName = $"_{publicName[0].ToString().ToLower()}{publicName.Substring(1)}";

                    codeWriter.WriteLine($"private Template<{property.Type}> {privateName} = new Constant<{property.Type}>(default);");

                    codeWriter.Write($"public ");
                    if (property.IsRequired) {
                        codeWriter.Write("required ");
                    }
                    codeWriter.WriteLine($"Template<{property.Type}> {publicName}");
                    codeWriter.WriteLine("{");
                    codeWriter.Indent++;
                    codeWriter.WriteLine($"get => {privateName};");
                    codeWriter.WriteLine("init");
                    codeWriter.WriteLine("{");
                    codeWriter.Indent++;
                    codeWriter.WriteLine($"{privateName} = value;");
                    codeWriter.WriteLine($"value.SetContext(this, \"{publicName}\");");
                    codeWriter.Indent--;
                    codeWriter.WriteLine("}");

                    codeWriter.Indent--;
                    codeWriter.WriteLine("}");

                    codeWriter.WriteLine();
                }

                codeWriter.Indent--;
                codeWriter.WriteLine("}");
            
                codeWriter.Indent--;
                codeWriter.WriteLine("}");

                codeWriter.Close();
                var sourceText = SourceText.From(textWriter.ToString(), Encoding.UTF8);
                context.AddSource($"{model.ClassName}.g.cs", sourceText);
            });
        }

        private static EquatableList<PropertyModel> GetPropertyModels(INamedTypeSymbol targetTypeSymbol) {
            return [.. 
                targetTypeSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(prop => prop.SetMethod != null)
                    .Select(prop => new PropertyModel(
                        prop.Name,
                        prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        prop.IsRequired
                    ))
            ];
        }

        private record PropertyModel(string Name, string Type, bool IsRequired);

        private record TemplateModel(string Namespace, string ClassName, string TargetClassName, EquatableList<PropertyModel> Properties);
    }
}
