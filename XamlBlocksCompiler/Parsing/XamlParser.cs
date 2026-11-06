using System.Xml;
using System.Xml.Linq;
using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlBlocksCompiler.Parsing;

public static class XamlParser
{
    public static XamlElementSyntax Parse(string xaml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xaml);

        using var reader = XmlReader.Create(
            new StringReader(xaml),
            new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Prohibit
            });

        var document = XDocument.Load(reader);

        return ParseElement(document.Root ?? throw new XmlException("XAML has no root element."));
    }

    private static XamlElementSyntax ParseElement(XElement element)
    {
        var syntax = GeneratedXamlControlRegistry.CreateElement(element.Name.LocalName)
            ?? throw new NotSupportedException($"Unknown XAML element: {element.Name}");

        foreach (var attribute in element.Attributes())
        {
            if (attribute.IsNamespaceDeclaration)
                continue;

            if (!GeneratedXamlControlRegistry.ReadAttribute(syntax, attribute.Name.LocalName, attribute.Value))
            {
                if (!GeneratedXamlControlRegistry.ReadContent(syntax, attribute.Value))
                    throw new NotSupportedException($"Unsupported attribute: {attribute.Name}");
            }
        }

        switch (syntax)
        {
            case XamlPanelSyntax panel:
                foreach (var childElement in element.Elements())
                {
                    if (childElement.Name.LocalName.Contains('.'))
                        throw new NotSupportedException("Property elements are not supported yet.");

                    panel.Children.Add(ParseElement(childElement));
                }
                break;

            case XamlContentSyntax content:
                var child = element.Elements().FirstOrDefault();

                if (child is not null)
                    content.Content = ParseElement(child);
                else if (!string.IsNullOrWhiteSpace(element.Value))
                    content.Content = element.Value;

                break;
        }

        return syntax;
    }
}