namespace XamlBlocksCompiler.Types;

public sealed class XamlPropertySymbol(string name, Type propertyType)
{
    public string Name { get; } = name;
    public Type PropertyType { get; } = propertyType;
}
