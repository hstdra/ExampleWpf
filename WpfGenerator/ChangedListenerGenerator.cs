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
public class ChangedListenerAttribute : Attribute
{
}

class ChangedListenerReceiver : ISyntaxContextReceiver
{
    public Dictionary<ClassDeclarationSyntax, List<IFieldSymbol>> ChangedListenerClasses { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            var hasChangedListenerAttribute = classDeclaration.AttributeLists
                .Any(x => x.Attributes.Any(y => y.Name.ToString() == "ChangedListener"));
            if (!hasChangedListenerAttribute) return;

            var fields = new List<IFieldSymbol>();
            ChangedListenerClasses.Add(classDeclaration, fields);

            foreach (var member in classDeclaration.Members)
            {
                if (member is not FieldDeclarationSyntax fieldDeclaration) continue;

                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    var field = context.SemanticModel.GetDeclaredSymbol(variable);
                    if (field is IFieldSymbol {DeclaredAccessibility: Accessibility.Private} fieldSymbol)
                    {
                        fields.Add(fieldSymbol);
                    }
                }
            }
        }
    }
}

[Generator]
public sealed class ChangedListenerGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ChangedListenerReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not ChangedListenerReceiver syntaxReceiver) return;

        Parallel.ForEach(syntaxReceiver.ChangedListenerClasses, changedListenerClass =>
        {
            var containingClass = changedListenerClass.Key.Identifier.ToString();
            var fieldSymbols = changedListenerClass.Value;
            var @namespace = GetNamespace(changedListenerClass.Key);
            var source = GenerateClass(containingClass, @namespace, fieldSymbols);
            context.AddSource($"{containingClass}.changed-listener-generated.cs",
                SourceText.From(source, Encoding.UTF8));
        });
    }

    private string GenerateClass(string className, string @namespace,
        List<IFieldSymbol> fieldSymbols)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine("using System.Collections.Generic;");
        classBuilder.AppendLine();
        classBuilder.AppendLine($"namespace {@namespace};");
        classBuilder.AppendLine();

        classBuilder.AppendLine($"public partial class {className}");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"\tpublic Action<{className}>? OnChanged;");

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

    private string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    private string NormalizePropertyName(string fieldName)
    {
        return Regex.Replace(fieldName, "_[a-z]",
            m => m.ToString().TrimStart('_').ToUpper());
    }
}