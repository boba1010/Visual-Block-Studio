namespace XamlBlocksCompiler.Types;

public sealed class XamlTypeRegistry
{
    private readonly Dictionary<(string NamespaceUri, string Name), XamlTypeSymbol> _types = [];

    public void Register(XamlTypeSymbol type)
    {
        ArgumentNullException.ThrowIfNull(type);

        _types.Add((type.NamespaceUri, type.Name), type);
    }

    public bool TryResolve(string namespaceUri, string name, out XamlTypeSymbol? type)
    {
        return _types.TryGetValue((namespaceUri, name), out type);
    }
}
