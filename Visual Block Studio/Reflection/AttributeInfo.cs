using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class AttributeInfo(AttributeData data)
{
    public string Name =>
        data.AttributeClass?.Name ?? string.Empty;

    public string FullName =>
        data.AttributeClass?.ToDisplayString() ?? string.Empty;
}