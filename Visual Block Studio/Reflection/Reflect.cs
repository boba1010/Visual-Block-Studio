using Microsoft.CodeAnalysis;

namespace Reflection;

public static class Reflect
{
    private static Compilation? _compilation;

    public static void SetCompilation(Compilation compilation)
    {
        _compilation = compilation;
    }

    public static Type Of<T>()
    {
        if (_compilation is null)
            throw new InvalidOperationException("Compilation not initialized.");

        var symbol = _compilation.GetTypeByMetadataName(
            typeof(T).FullName!);

        if (symbol is null)
            throw new InvalidOperationException($"Type {typeof(T).FullName} was not found.");

        return new Type(symbol);
    }
}
