using System.Text.Json;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlBlocksCompiler.Parsing;

public sealed class XamlBlockParser
{
    public XamlBlockSyntax Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);
        JsonElement source = document.RootElement;

        if (source.ValueKind != JsonValueKind.Object)
            throw new JsonException("The document root must be an object.");

        int version = GetRequiredInt32(source, "version");

        if (version != 1)
            throw new JsonException($"Unsupported .xamlVblock version: {version}.");

        JsonElement root = GetRequiredProperty(source, "root");
        XamlBlockSyntax syntax = ParseNode(root);

        if (syntax is not XamlElementSyntax)
            throw new JsonException("The document root must be an element.");

        return syntax;
    }

    private static XamlBlockSyntax ParseNode(JsonElement node)
    {
        if (node.ValueKind != JsonValueKind.Object)
            throw new JsonException("A syntax node must be an object.");

        string type = GetRequiredString(node, "type");
        string name = GetRequiredString(node, "name");

        switch (type)
        {
            case "element":
                {
                    XamlElementSyntax element = name switch
                    {
                        "Application" => new XamlApplicationSyntax(),
                        "Window" => new XamlWindowSyntax(),
                        "Canvas" => new XamlCanvasSyntax(),
                        "Grid" => new XamlGridSyntax(),
                        //"Button" => new XamlButtonSyntax(),
                        //"Border" => new XamlBorderSyntax(),
                        //_ => new XamlElementSyntaxImpl(name)
                        _ => new(name)
                    };

                    foreach (JsonElement ns in GetOptionalArray(node, "namespaces"))
                    {
                        element.Namespaces.Add(
                            new XamlNamespaceSyntax(GetRequiredStringAllowEmpty(ns, "prefix"), GetRequiredString(ns, "uri")));
                    }

                    foreach (JsonElement attribute in GetOptionalArray(node, "attributes"))
                    {
                        element.Attributes.Add(
                            new XamlAttributeSyntax(GetRequiredString(attribute, "name"), GetRequiredString(attribute, "value")));
                    }

                    switch (element)
                    {
                        case XamlPanelSyntax panel:
                            foreach (JsonElement child in GetOptionalArray(node, "children"))
                            {
                                if (ParseNode(child) is not XamlElementSyntax childElement)
                                    throw new JsonException("Panel children must be elements.");

                                panel.Children.Add(childElement);
                            }
                            break;

                        case XamlContentSyntax content:
                            if (node.TryGetProperty("content", out JsonElement contentNode))
                            {
                                if (ParseNode(contentNode) is not XamlElementSyntax contentElement)
                                    throw new JsonException("Content must be an element.");

                                content.Content = contentElement;
                            }
                            break;
                    }

                    return element;
                }

            case "property":
                {
                    var property = new XamlPropertySyntax(name);

                    foreach (JsonElement child in GetOptionalArray(node, "children"))
                        property.Children.Add((XamlElementSyntax)ParseNode(child));

                    return property;
                }

            default:
                throw new JsonException($"Unknown syntax node type '{type}'.");
        }
    }

    private static JsonElement GetRequiredProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value))
            throw new JsonException($"Missing required property '{name}'.");

        return value;
    }

    private static string GetRequiredStringAllowEmpty(JsonElement element, string name)
    {
        JsonElement value = GetRequiredProperty(element, name);

        if (value.ValueKind != JsonValueKind.String)
            throw new JsonException($"Property '{name}' must be a string.");

        return value.GetString()!;
    }

    private static string GetRequiredString(JsonElement element, string name)
    {
        JsonElement value = GetRequiredProperty(element, name);

        if (value.ValueKind != JsonValueKind.String)
            throw new JsonException($"Property '{name}' must be a string.");

        string? result = value.GetString();

        if (string.IsNullOrWhiteSpace(result))
            throw new JsonException($"Property '{name}' cannot be empty.");

        return result;
    }

    private static int GetRequiredInt32(JsonElement element, string name)
    {
        JsonElement value = GetRequiredProperty(element, name);

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int result))
            throw new JsonException($"Property '{name}' must be a 32-bit integer.");

        return result;
    }

    private static JsonElement.ArrayEnumerator GetOptionalArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
            return default;

        if (value.ValueKind != JsonValueKind.Array)
            throw new JsonException($"Property '{name}' must be an array.");

        return value.EnumerateArray();
    }
}
