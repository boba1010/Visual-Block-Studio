using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace XamlBlocksCompiler.SourceGeneration;

[Generator]
public sealed class XamlMetadataGenerator : IIncrementalGenerator
{
    private const string AttributeName = "XamlBlocksCompiler.Syntax.XamlControlAttribute";

    private static readonly HashSet<string> ExcludedProperties =
    [
        "Namespaces",
        "Attributes",
        "Name",
        "Children",
        "Content"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controls = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
            .Where(static type => IsSupportedControl(type));

        // Generate metadata for each individual control.
        context.RegisterSourceOutput(controls, static (spc, type) =>
        {
            var properties = GetProperties(type)
                .Where(static property =>
                    property is
                    {
                        IsStatic: false,
                        IsIndexer: false,
                        DeclaredAccessibility: Accessibility.Public,
                        GetMethod.DeclaredAccessibility: Accessibility.Public
                    })
                .Where(static property => !ExcludedProperties.Contains(property.Name))
                .ToArray();

            var source = GenerateMetadata(type, properties);

            spc.AddSource(GetHintName(type), SourceText.From(source, Encoding.UTF8));
        });

        // Generate the central control registry.
        context.RegisterSourceOutput(controls.Collect(), static (spc, types) =>
        {
            var source = GenerateRegistry(types);

            spc.AddSource("XamlControlRegistry.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static bool IsSupportedControl(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class
            && !type.IsStatic
            && !type.IsGenericType
            && type.ContainingType is null;
    }

    private static string GetHintName(INamedTypeSymbol type)
    {
        string fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string safeName = new([.. fullName.Select(static c => char.IsLetterOrDigit(c) ? c : '_')]);

        return $"{safeName}.XamlMetadata.g.cs";
    }

    private static string GetMetadataTypeName(INamedTypeSymbol type)
    {
        string ns = type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString() + ".";

        return $"global::{ns}{type.Name}XamlMetadata";
    }

    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>();

        // Derived properties take precedence over inherited properties.
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (seen.Add(property.Name))
                    yield return property;
            }
        }
    }

    private static string GenerateValueExpression(IPropertySymbol property)
    {
        string typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string propertyName = EscapeIdentifier(property.Name);
        string value = $"control.{propertyName}";

        return typeName switch
        {
            "global::XamlBlocksCompiler.Models.Color" => $"{value}.ToHex()",
            "global::XamlBlocksCompiler.Models.Alignment"
                when property.Name == "VerticalAlignment" =>
                $$"""
                ({{value}}.ToString() switch
                {
                    "Center" => "Center",
                    "Start" => "Top",
                    "End" => "Bottom",
                    "Stretch" => "Stretch",
                    _ => throw new global::System.ArgumentOutOfRangeException()
                })
                """,

            "global::XamlBlocksCompiler.Models.Alignment"
                when property.Name == "HorizontalAlignment" =>
                $$"""
                ({{value}}.ToString() switch
                {
                    "Center" => "Center",
                    "Start" => "Left",
                    "End" => "Right",
                    "Stretch" => "Stretch",
                    _ => throw new global::System.ArgumentOutOfRangeException()
                })
                """,

            _ => $"global::System.Convert.ToString({value}, " + "global::System.Globalization.CultureInfo.InvariantCulture)"
        };
    }

    private static string GenerateMetadata(INamedTypeSymbol type, IPropertySymbol[] properties)
    {
        string ns = type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString();

        string namespaceDeclaration = string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};";

        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string propertyNames = string.Join(
            ",\n        ",
            properties.Select(property => SymbolDisplay.FormatLiteral(property.Name, quote: true)));

        string elementName = GetElementName(type);

        string attributeWriters = string.Join(
            "\n        ",
            properties.Select(GenerateAttributeWriter));

        return $$"""
        // <auto-generated/>
        #nullable enable

        {{namespaceDeclaration}}

        public static class {{type.Name}}XamlMetadata
        {
            public const string ElementName = {{SymbolDisplay.FormatLiteral(elementName, quote: true)}};

            public static readonly string[] PropertyNames =
            {
                {{propertyNames}}
            };

            public static void WriteAttributes(global::System.Xml.XmlWriter writer, {{typeName}} control)
            {
                {{attributeWriters}}
            }
        }
        """;
    }

    private static string GetElementName(INamedTypeSymbol type)
    {
        var attribute = type.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == AttributeName);

        if (attribute is not null &&
            attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is string name &&
            !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        const string suffix = "Syntax";

        return type.Name.EndsWith(suffix, StringComparison.Ordinal)
            ? type.Name.Substring(0, type.Name.Length - suffix.Length)
            : type.Name;
    }

    private static string GenerateAttributeWriter(IPropertySymbol property)
    {
        string propertyName = SymbolDisplay.FormatLiteral(property.Name, quote: true);

        string valueExpression = GenerateValueExpression(property);

        if (IsNumericType(property.Type))
        {
            return $$"""
            {
                var value = {{valueExpression}};

                if (value is not null &&
                    global::System.Double.TryParse(value, global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture, out var number) &&
                    !global::System.Double.IsNaN(number) &&
                    !global::System.Double.IsInfinity(number))
                {
                    writer.WriteAttributeString({{propertyName}}, value);
                }
            }
            """;
        }

        return $$"""
        writer.WriteAttributeString({{propertyName}}, {{valueExpression}});
        """;
    }

    private static bool IsNumericType(ITypeSymbol type)
    {
        // Support nullable numeric properties as well.
        if (type is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            type = nullable.TypeArguments[0];

        return type.SpecialType is
            SpecialType.System_Double or
            SpecialType.System_Single or
            SpecialType.System_Int32 or
            SpecialType.System_Int64 or
            SpecialType.System_Int16 or
            SpecialType.System_Byte or
            SpecialType.System_Decimal;
    }

    private static string EscapeIdentifier(string identifier)
    {
        return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ? "@" + identifier : identifier;
    }

    private static string GenerateRegistry(ImmutableArray<INamedTypeSymbol> types)
    {
        // Derived controls must appear before their base controls.
        var orderedTypes = types
            .OrderByDescending(GetInheritanceDepth)
            .ThenBy(static type => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
            .ToArray();

        string elementNameCases = string.Join("\n",
            orderedTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string metadataName = GetMetadataTypeName(type);

                return $"{typeName} => {metadataName}.ElementName,";
            }));

        string attributeCases = string.Join(
            "\n",
            orderedTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

                string metadataName = GetMetadataTypeName(type);

                return $$"""
                    case {{typeName}} control:
                        {{metadataName}}.WriteAttributes(writer, control);
                        return true;
                    """;
            }));

        return $$"""
        // <auto-generated/>
        #nullable enable

        namespace XamlBlocksCompiler.Generation;

        public static class GeneratedXamlControlRegistry
        {
            public static string? GetElementName(global::XamlBlocksCompiler.Syntax.XamlElementSyntax element)
            {
                return element switch
                {
                    {{elementNameCases}}

                    _ => null
                };
            }

            public static bool WriteAttributes(global::System.Xml.XmlWriter writer, global::XamlBlocksCompiler.Syntax.XamlElementSyntax element)
            {
                switch (element)
                {
                    {{attributeCases}}

                    default:
                        return false;
                }
            }
        }
        """;
    }

    private static int GetInheritanceDepth(INamedTypeSymbol type)
    {
        int depth = 0;

        for (var current = type; current is not null; current = current.BaseType)
            depth++;

        return depth;
    }
}