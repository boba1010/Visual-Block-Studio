using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class ParameterInfo(IParameterSymbol symbol)
{
    public string Name => symbol.Name;

    public Type ParameterType => new(symbol.Type);

    public bool IsOptional => symbol.IsOptional;

    public bool HasDefaultValue => symbol.HasExplicitDefaultValue;

    public object? DefaultValue => symbol.ExplicitDefaultValue;
}