namespace XamlBlocksCompiler.Syntax;

public sealed class XamlNamespaceSyntax(string prefix, string namespaceUri) : XamlBlockSyntax
{
    public string Prefix { get; } = prefix;
    public string NamespaceUri { get; } = namespaceUri;
}
