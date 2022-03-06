// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Text.RegularExpressions;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.CodeAnalysis.Text;
//
// namespace WpfGenerator;
//
// [AttributeUsage(AttributeTargets.Field)]
// public class ChangePublisherAttribute : Attribute
// {
// }
//
// [Generator]
// public class ChangePublisherPropertyGenerator : ISourceGenerator
// {
//     public void Initialize(GeneratorInitializationContext context)
//     {
//         context.RegisterForSyntaxNotifications(() => new FieldSyntaxReceiver());
//     }
//
//     public void Execute(GeneratorExecutionContext context)
//     {
//         return;
//         if (context.SyntaxContextReceiver is not FieldSyntaxReceiver syntaxReceiver) return;
//
//         foreach (var containingClassGroup in syntaxReceiver.IdentifiedFields.GroupBy(x => x.ContainingType))
//         {
//             var containingClass = containingClassGroup.Key;
//             var fieldSymbols = containingClassGroup.ToList();
//             var @namespace = containingClass.ContainingNamespace;
//
//             var source = GenerateClass(containingClass, @namespace, fieldSymbols);
//             context.AddSource($"{containingClass.Name}_AutoNotify.generated.cs",
//                 SourceText.From(source, Encoding.UTF8));
//         }
//     }
//
//     private string GenerateClass(INamedTypeSymbol @class, INamespaceSymbol namespaceSymbol,
//         List<IFieldSymbol> fieldSymbols)
//     {
//         var classBuilder = new StringBuilder();
//         classBuilder.AppendLine("using System;");
//         classBuilder.AppendLine("using System.Collections.Generic;");
//         classBuilder.AppendLine();
//         classBuilder.AppendLine($"namespace {namespaceSymbol.ToDisplayString()};");
//         classBuilder.AppendLine();
//
//         classBuilder.AppendLine($"public partial class {@class.Name}");
//         classBuilder.AppendLine("{");
//
//         foreach (var field in fieldSymbols)
//         {
//             var fieldName = field.Name;
//             var fieldType = field.Type;
//
//             classBuilder.AppendLine();
//             classBuilder.AppendLine($"\tpublic Action<{fieldType}> On{NormalizePropertyName(fieldName)}Changed;");
//             classBuilder.AppendLine($"\tpublic {fieldType} {NormalizePropertyName(fieldName)}");
//             classBuilder.AppendLine("\t{");
//             classBuilder.AppendLine($"\t\tget => {fieldName};");
//             classBuilder.AppendLine($"\t\tset");
//             classBuilder.AppendLine("\t\t{");
//             classBuilder.AppendLine(
//                 $"\t\t\tif (!EqualityComparer<{fieldType}>.Default.Equals({fieldName}, value)) return;");
//             classBuilder.AppendLine($"\t\t\t{fieldName} = value;");
//             classBuilder.AppendLine($"\t\t\tOn{NormalizePropertyName(fieldName)}Changed?.Invoke(value);");
//             classBuilder.AppendLine("\t\t}");
//             classBuilder.AppendLine("\t}");
//         }
//
//         classBuilder.AppendLine("}");
//
//         return classBuilder.ToString();
//     }
//
//     private string NormalizePropertyName(string fieldName)
//     {
//         return Regex.Replace(fieldName, "_[a-z]",
//             delegate(Match m) { return m.ToString().TrimStart('_').ToUpper(); });
//     }
//
//     private class FieldSyntaxReceiver : ISyntaxContextReceiver
//     {
//         public List<IFieldSymbol> IdentifiedFields { get; } = new();
//
//         public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
//         {
//             if (context.Node is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.AttributeLists.Any())
//             {
//                 var variableDeclaration = fieldDeclaration.Declaration.Variables;
//                 foreach (var variable in variableDeclaration)
//                 {
//                     var field = context.SemanticModel.GetDeclaredSymbol(variable);
//                     if (field is IFieldSymbol fieldInfo && fieldInfo.GetAttributes().Any(x =>
//                             x.AttributeClass?.ToDisplayString() ==
//                             $"{nameof(WpfGenerator)}.{nameof(ChangePublisherAttribute)}"))
//                     {
//                         IdentifiedFields.Add(fieldInfo);
//                     }
//                 }
//             }
//         }
//     }
// }