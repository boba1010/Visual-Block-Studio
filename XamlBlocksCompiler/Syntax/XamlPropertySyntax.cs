namespace XamlBlocksCompiler.Syntax;

public sealed class XamlPropertySyntax(string propertyName) : XamlBlockSyntax
{
    public string PropertyName { get; } = propertyName;
    public List<XamlBlockSyntax> Children { get; } = [];
}