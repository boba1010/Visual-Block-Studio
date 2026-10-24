using System.Globalization;
using System.Xml;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlBlocksCompiler.Generation;

public sealed class XamlCodeGenerator
{
    public string Generate(XamlBlockSyntax root)
    {
        ArgumentNullException.ThrowIfNull(root);

        using var output = new StringWriter();
        using var writer = XmlWriter.Create(output, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            OmitXmlDeclaration = true,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace
        });

        WriteNode(writer, root);

        writer.Flush();
        return output.ToString();
    }

    private static void WriteNode(XmlWriter writer, object node)
    {
        switch (node)
        {
            case XamlElementSyntax element:
                WriteElement(writer, element);
                break;

            case XamlPropertySyntax property:
                WriteProperty(writer, property);
                break;

            default:
                throw new NotSupportedException($"Unsupported XAML syntax node: {node.GetType().Name}");
        }
    }

    private static void WriteElement(XmlWriter writer, XamlElementSyntax element)
    {
        string? defaultNamespace = element.Namespaces.FirstOrDefault(ns => string.IsNullOrEmpty(ns.Prefix))?.NamespaceUri;

        if (defaultNamespace is null)
            writer.WriteStartElement(element.Name);
        else
            writer.WriteStartElement(element.Name, defaultNamespace);

        foreach (var ns in element.Namespaces)
        {
            if (string.IsNullOrEmpty(ns.Prefix))
                continue;

            writer.WriteAttributeString("xmlns", ns.Prefix, "http://www.w3.org/2000/xmlns/", ns.NamespaceUri);
        }

        WriteGeneratedAttributes(writer, element);

        switch (element)
        {
            case XamlPanelSyntax panel:
                foreach (var child in panel.Children)
                    WriteNode(writer, child);
                break;

            case XamlWindowSyntax window when window.Content is not null:
                if (window.Content is XamlElementSyntax or XamlPropertySyntax)
                    WriteNode(writer, window.Content);
                else
                {
                    writer.WriteAttributeString(
                        "Content",
                        Convert.ToString(window.Content, System.Globalization.CultureInfo.InvariantCulture)
                    );
                }
                break;

            case XamlContentSyntax content when content.Content is not null:
                if (content.Content is XamlElementSyntax or XamlPropertySyntax)
                    WriteNode(writer, content.Content);
                else
                {
                    writer.WriteAttributeString(
                        "Content", 
                        Convert.ToString(content.Content, System.Globalization.CultureInfo.InvariantCulture)
                    );
                }
                break;
        }

        writer.WriteEndElement();
    }

    private static void WriteGeneratedAttributes(XmlWriter writer, XamlElementSyntax element)
    {
        switch (element)
        {
            case XamlButtonSyntax button:
                XamlButtonSyntaxXamlMetadata.WriteAttributes(writer, button);
                break;

            case XamlGridSyntax grid:
                XamlGridSyntaxXamlMetadata.WriteAttributes(writer, grid);
                break;

            case XamlWindowSyntax window:
                XamlWindowSyntaxXamlMetadata.WriteAttributes(writer, window);
                break;
        }
    }

    private static void WriteProperty(XmlWriter writer, XamlPropertySyntax property)
    {
        writer.WriteStartElement(property.PropertyName);

        foreach (var child in property.Children)
            WriteNode(writer, child);

        writer.WriteEndElement();
    }
}
