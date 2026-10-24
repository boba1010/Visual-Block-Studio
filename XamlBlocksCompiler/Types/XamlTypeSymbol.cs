namespace XamlBlocksCompiler.Types;

public sealed class XamlTypeSymbol(string name, string namespaceUri)
{
    public string Name { get; } = name;
    public string NamespaceUri { get; } = namespaceUri;
    public List<XamlPropertySymbol> Properties { get; } = [];
}
