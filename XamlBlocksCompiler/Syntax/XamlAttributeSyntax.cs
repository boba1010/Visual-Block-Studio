namespace XamlBlocksCompiler.Syntax;

public sealed class XamlAttributeSyntax(string name, string value)
{
    public string Name { get; } = name;
    public string Value { get; } = value;
}
