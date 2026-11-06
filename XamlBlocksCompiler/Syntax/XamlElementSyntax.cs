namespace XamlBlocksCompiler.Syntax;

public class XamlElementSyntax(string name) : XamlBlockSyntax
{
    public string Name { get; } = name;

    public List<XamlNamespaceSyntax> Namespaces { get; } = [];
}