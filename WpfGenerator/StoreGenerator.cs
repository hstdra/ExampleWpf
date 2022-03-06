using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WpfGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class StoreAttribute : Attribute
{
}

class StoreReceiver : ISyntaxContextReceiver
{
    public List<IFieldSymbol> StoreFields { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            var hasStoreAttribute = classDeclaration.AttributeLists
                .Any(x => x.Attributes.Any(y => y.Name.ToString() == "Store"));
            if (!hasStoreAttribute) return;

            foreach (var member in classDeclaration.Members)
            {
                if (member is not FieldDeclarationSyntax fieldDeclaration) continue;

                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    var field = context.SemanticModel.GetDeclaredSymbol(variable);
                    if (field is IFieldSymbol {DeclaredAccessibility: Accessibility.Private} fieldSymbol)
                    {
                        StoreFields.Add(fieldSymbol);
                    }
                }
            }
        }
    }
}

[Generator]
public sealed class StoreGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new StoreReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not StoreReceiver syntaxReceiver) return;

        Parallel.ForEach(syntaxReceiver.StoreFields.GroupBy(x => x.ContainingType), containingClassGroup =>
        {
            var containingClass = containingClassGroup.Key;
            var fieldSymbols = containingClassGroup.ToList();
            var @namespace = containingClass.ContainingNamespace;

            var source = GenerateClass(containingClass, @namespace, fieldSymbols);
            context.AddSource($"{containingClass.Name}.store-generated.cs",
                SourceText.From(source, Encoding.UTF8));
        });
    }

    private string GenerateClass(INamedTypeSymbol @class, INamespaceSymbol namespaceSymbol,
        List<IFieldSymbol> fieldSymbols)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine("using System.Collections.Generic;");
        classBuilder.AppendLine();
        classBuilder.AppendLine($"namespace {namespaceSymbol.ToDisplayString()};");
        classBuilder.AppendLine();

        classBuilder.AppendLine($"public partial class {@class.Name}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"\tpublic Action<{@class.Name}>? OnChanged;");

        foreach (var field in fieldSymbols)
        {
            var fieldName = field.Name;
            var fieldType = field.Type;

            classBuilder.AppendLine();
            classBuilder.AppendLine($"\tpublic Action<{fieldType}>? On{NormalizePropertyName(fieldName)}Changed;");
            classBuilder.AppendLine($"\tpublic {fieldType} {NormalizePropertyName(fieldName)}");
            classBuilder.AppendLine("\t{");
            classBuilder.AppendLine($"\t\tget => {fieldName};");
            classBuilder.AppendLine($"\t\tset");
            classBuilder.AppendLine("\t\t{");
            classBuilder.AppendLine(
                $"\t\t\tif (EqualityComparer<{fieldType}>.Default.Equals({fieldName}, value)) return;");
            classBuilder.AppendLine($"\t\t\t{fieldName} = value;");
            classBuilder.AppendLine($"\t\t\tOn{NormalizePropertyName(fieldName)}Changed?.Invoke(value);");
            classBuilder.AppendLine($"\t\t\tOnChanged?.Invoke(this);");
            classBuilder.AppendLine("\t\t}");
            classBuilder.AppendLine("\t}");
        }

        classBuilder.AppendLine("}");

        return classBuilder.ToString();
    }

    private string NormalizePropertyName(string fieldName)
    {
        return Regex.Replace(fieldName, "_[a-z]",
            m => m.ToString().TrimStart('_').ToUpper());
    }
}