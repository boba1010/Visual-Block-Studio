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

    private const string ContentSyntaxName = "XamlBlocksCompiler.Syntax.Controls.XamlContentSyntax";

    private static readonly HashSet<string> ExcludedProperties =
    [
        "Namespaces",
        "Attributes",
        "Name",
        "Children"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controls = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeName,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
            .Where(static type => IsSupportedControl(type));

        context.RegisterSourceOutput(controls, static (spc, type) =>
        {
            var properties = GetProperties(type)
                .Where(IsReadableProperty)
                .Where(static property => !ExcludedProperties.Contains(property.Name))
                .ToArray();

            string source = GenerateMetadata(type, properties);

            spc.AddSource(GetHintName(type), SourceText.From(source, Encoding.UTF8));
        });

        context.RegisterSourceOutput(controls.Collect(), static (spc, types) =>
        {
            string source = GenerateRegistry(types);

            spc.AddSource("XamlControlRegistry.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    // ------------------------------------------------------------
    // Control discovery
    // ------------------------------------------------------------

    private static bool IsSupportedControl(INamedTypeSymbol type)
    {
        return type.TypeKind == TypeKind.Class
            && !type.IsStatic
            && !type.IsAbstract
            && !type.IsGenericType
            && type.ContainingType is null
            && HasAccessibleParameterlessConstructor(type);
    }

    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol type)
    {
        return type.InstanceConstructors.Any(static constructor =>
            constructor.Parameters.Length == 0 &&
            (constructor.DeclaredAccessibility == Accessibility.Public ||
             constructor.DeclaredAccessibility == Accessibility.Internal ||
             constructor.DeclaredAccessibility == Accessibility.ProtectedOrInternal));
    }

    private static bool IsReadableProperty(IPropertySymbol property)
    {
        return !property.IsStatic
            && !property.IsIndexer
            && property.DeclaredAccessibility == Accessibility.Public
            && property.GetMethod?.DeclaredAccessibility == Accessibility.Public;
    }

    private static string GetHintName(INamedTypeSymbol type)
    {
        string fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string safeName = new(fullName.Select(static c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

        return $"{safeName}.XamlMetadata.g.cs";
    }

    private static string GetMetadataTypeName(INamedTypeSymbol type)
    {
        string ns = type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString() + ".";

        return $"global::{ns}{type.Name}XamlMetadata";
    }

    private static string GetTypeName(ITypeSymbol type)
    {
        return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    // ------------------------------------------------------------
    // Property discovery
    // ------------------------------------------------------------

    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>();

        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (seen.Add(property.Name))
                    yield return property;
            }
        }
    }

    private static bool IsWritableProperty(IPropertySymbol property)
    {
        return property.SetMethod?.DeclaredAccessibility == Accessibility.Public;
    }

    // ------------------------------------------------------------
    // Content control detection
    // ------------------------------------------------------------

    private static bool IsContentControl(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == ContentSyntaxName)
                return true;
        }

        return false;
    }

    private static IPropertySymbol? GetContentProperty(INamedTypeSymbol type)
    {
        return GetProperties(type).FirstOrDefault(static property => property.Name == "Content");
    }

    // ------------------------------------------------------------
    // Element names
    // ------------------------------------------------------------

    private static string GetElementName(INamedTypeSymbol type)
    {
        var attribute = type.GetAttributes()
            .FirstOrDefault(static attribute => attribute.AttributeClass?.ToDisplayString() == AttributeName);

        if (attribute is not null &&
            attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is string name &&
            !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        string result = type.Name;

        if (result.EndsWith("Syntax", StringComparison.Ordinal))
            result = result.Substring(0, result.Length - "Syntax".Length);

        if (result.StartsWith("Xaml", StringComparison.Ordinal))
            result = result.Substring("Xaml".Length);

        return result;
    }

    // ------------------------------------------------------------
    // Value serialization
    // ------------------------------------------------------------

    private static string GenerateValueExpression(IPropertySymbol property)
    {
        string propertyName = EscapeIdentifier(property.Name);
        string value = $"control.{propertyName}";
        string typeName = GetTypeName(property.Type);

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

            _ => $"global::System.Convert.ToString({value}, global::System.Globalization.CultureInfo.InvariantCulture)"
        };
    }

    private static bool IsNumericType(ITypeSymbol type)
    {
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
                    global::System.Double.TryParse(
                        value,
                        global::System.Globalization.NumberStyles.Float,
                        global::System.Globalization.CultureInfo.InvariantCulture,
                        out var number) &&
                    !global::System.Double.IsNaN(number) &&
                    !global::System.Double.IsInfinity(number))
                {
                    writer.WriteAttributeString({{propertyName}}, value);
                }
            }
            """;
        }

        return $"writer.WriteAttributeString({propertyName}, {valueExpression});";
    }

    // ------------------------------------------------------------
    // Attribute deserialization
    // ------------------------------------------------------------

    private static string? GenerateAttributeReader(IPropertySymbol property)
    {
        string name = SymbolDisplay.FormatLiteral(property.Name, true);
        string identifier = EscapeIdentifier(property.Name);
        string type = GetTypeName(property.Type);

        string? assignment = type switch
        {
            "global::System.String" => $"control.{identifier} = value;",

            "global::XamlBlocksCompiler.Models.Alignment"
                when property.Name == "VerticalAlignment" =>
                """
                control.VerticalAlignment = value switch
                {
                    "Center" => global::XamlBlocksCompiler.Models.Alignment.Center,
                    "Top" => global::XamlBlocksCompiler.Models.Alignment.Start,
                    "Bottom" => global::XamlBlocksCompiler.Models.Alignment.End,
                    "Stretch" => global::XamlBlocksCompiler.Models.Alignment.Stretch,
                    _ => throw new global::System.FormatException(
                        $"Invalid VerticalAlignment: {value}")
                };
                """,

            "global::XamlBlocksCompiler.Models.Alignment"
                when property.Name == "HorizontalAlignment" =>
                """
                control.HorizontalAlignment = value switch
                {
                    "Center" => global::XamlBlocksCompiler.Models.Alignment.Center,
                    "Left" => global::XamlBlocksCompiler.Models.Alignment.Start,
                    "Right" => global::XamlBlocksCompiler.Models.Alignment.End,
                    "Stretch" => global::XamlBlocksCompiler.Models.Alignment.Stretch,
                    _ => throw new global::System.FormatException(
                        $"Invalid HorizontalAlignment: {value}")
                };
                """,

            "global::XamlBlocksCompiler.Models.Color" =>
                $"control.{identifier} = global::XamlBlocksCompiler.Models.Color.FromHex(value);",

            _ when property.Type.SpecialType == SpecialType.System_Double =>
                $"control.{identifier} = double.Parse(value, global::System.Globalization.CultureInfo.InvariantCulture);",

            _ when property.Type.SpecialType == SpecialType.System_Single =>
                $"control.{identifier} = float.Parse(value, global::System.Globalization.CultureInfo.InvariantCulture);",

            _ when property.Type.SpecialType == SpecialType.System_Int32 =>
                $"control.{identifier} = int.Parse(value, global::System.Globalization.CultureInfo.InvariantCulture);",

            _ when property.Type.SpecialType == SpecialType.System_Int64 =>
                $"control.{identifier} = long.Parse(value, global::System.Globalization.CultureInfo.InvariantCulture);",

            _ when property.Type.SpecialType == SpecialType.System_Boolean => $"control.{identifier} = bool.Parse(value);",

            _ => null
        };

        if (assignment is null)
            return null;

        return $$"""
        case {{name}}:
            {{assignment}}
            return true;
        """;
    }

    // ------------------------------------------------------------
    // Content deserialization
    // ------------------------------------------------------------

    private static string GenerateContentReader(INamedTypeSymbol type, IPropertySymbol contentProperty)
    {
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string propertyName = EscapeIdentifier(contentProperty.Name);

        return $$"""
            public static bool ReadContent({{typeName}} control, object? content)
            {
                control.{{propertyName}} = content;
                return true;
            }
            """;
    }

    // ------------------------------------------------------------
    // Metadata generation
    // ------------------------------------------------------------

    private static string GenerateMetadata(INamedTypeSymbol type, IPropertySymbol[] properties)
    {
        string ns = type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString();

        string namespaceDeclaration = string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};";

        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Content remains discoverable in PropertyNames.
        string propertyNames = string.Join(",\n        ", properties.Select(property => SymbolDisplay.FormatLiteral(property.Name, true)));

        // Content is not an XML attribute.
        var attributeProperties = properties.Where(static property => property.Name != "Content").ToArray();

        string attributeWriters = string.Join("\n        ", attributeProperties.Select(GenerateAttributeWriter));

        var writableProperties = attributeProperties
            .Where(IsWritableProperty)
            .Select(GenerateAttributeReader)
            .Where(static reader => reader is not null)
            .ToArray();

        string attributeReaders = string.Join("\n", writableProperties);

        string contentReader = "";

        if (IsContentControl(type))
        {
            var contentProperty = GetContentProperty(type);

            if (contentProperty is not null && IsWritableProperty(contentProperty))
                contentReader = GenerateContentReader(type, contentProperty);
        }

        return $$"""
        // <auto-generated/>
        #nullable enable

        {{namespaceDeclaration}}

        public static class {{type.Name}}XamlMetadata
        {
            public const string ElementName = {{SymbolDisplay.FormatLiteral(GetElementName(type), true)}};

            public static readonly string[] PropertyNames =
            {
                {{propertyNames}}
            };

            public static void WriteAttributes(global::System.Xml.XmlWriter writer, {{typeName}} control)
            {
                {{attributeWriters}}
            }

            public static bool ReadAttribute({{typeName}} control, string name, string value)
            {
                switch (name)
                {
                    {{attributeReaders}}

                    default:
                        return false;
                }
            }

            {{contentReader}}
        }
        """;
    }

    // ------------------------------------------------------------
    // Registry generation
    // ------------------------------------------------------------

    private static string GenerateRegistry(ImmutableArray<INamedTypeSymbol> types)
    {
        var orderedTypes = types
            .OrderByDescending(GetInheritanceDepth)
            .ThenBy(static type => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
            .ToArray();

        string elementNameCases = string.Join(
            "\n",
            orderedTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string metadata = GetMetadataTypeName(type);

                return $"{typeName} => {metadata}.ElementName,";
            }));

        string writerCases = string.Join(
            "\n",
            orderedTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string metadata = GetMetadataTypeName(type);

                return $$"""
                case {{typeName}} control:
                    {{metadata}}.WriteAttributes(writer, control);
                    return true;
                """;
            }));

        string readerCases = string.Join(
            "\n",
            orderedTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string metadata = GetMetadataTypeName(type);

                return $$"""
                case {{typeName}} control:
                    return {{metadata}}.ReadAttribute(control, name, value);
                """;
            }));

        var contentTypes = orderedTypes
            .Where(IsContentControl)
            .Where(type =>
            {
                var property = GetContentProperty(type);
                return property is not null && IsWritableProperty(property);
            })
            .ToArray();

        string contentCases = string.Join(
            "\n",
            contentTypes.Select(type =>
            {
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string metadata = GetMetadataTypeName(type);

                return $$"""
                case {{typeName}} control:
                    return {{metadata}}.ReadContent(control, content);
                """;
            }));

        string factoryCases = string.Join(
            "\n",
            orderedTypes.Select(type =>
            {
                string metadata = GetMetadataTypeName(type);
                string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                return $"{metadata}.ElementName => new {typeName}(),";
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
                    {{writerCases}}

                    default:
                        return false;
                }
            }

            public static bool ReadAttribute(global::XamlBlocksCompiler.Syntax.XamlElementSyntax element, string name, string value)
            {
                switch (element)
                {
                    {{readerCases}}

                    default:
                        return false;
                }
            }

            public static bool ReadContent(global::XamlBlocksCompiler.Syntax.XamlElementSyntax element, object? content)
            {
                switch (element)
                {
                    {{contentCases}}

                    default:
                        return false;
                }
            }

            public static global::XamlBlocksCompiler.Syntax.XamlElementSyntax? CreateElement(string name)
            {
                return name switch
                {
                    {{factoryCases}}

                    _ => null
                };
            }
        }
        """;
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    private static int GetInheritanceDepth(INamedTypeSymbol type)
    {
        int depth = 0;

        for (var current = type; current is not null; current = current.BaseType)
            depth++;

        return depth;
    }

    private static string EscapeIdentifier(string identifier)
    {
        return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ? "@" + identifier : identifier;
    }
}