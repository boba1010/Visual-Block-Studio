namespace XamlBlocksCompiler.Syntax;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class XamlControlAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}