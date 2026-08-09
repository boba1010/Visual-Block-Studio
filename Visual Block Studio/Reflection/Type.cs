using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class Type(ITypeSymbol symbol)
{
    public string Name => symbol.Name;

    public string FullName =>
        symbol.ToDisplayString();

    public MethodInfo? GetMethod(string name)
    {
        var method = symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == name);

        return method is null ? null : new MethodInfo(method);
    }

    public MethodInfo[] GetMethods()
    {
        return symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => new MethodInfo(m))
            .ToArray();
    }

    public PropertyInfo? GetProperty(string name)
    {
        var property = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.Name == name);

        return property is null ? null : new PropertyInfo(property);
    }

    public PropertyInfo[] GetProperties()
    {
        return symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => new PropertyInfo(p))
            .ToArray();
    }

    public FieldInfo? GetField(string name)
    {
        var field = symbol.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.Name == name);

        return field is null ? null : new FieldInfo(field);
    }

    public FieldInfo[] GetFields()
    {
        return symbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Select(f => new FieldInfo(f))
            .ToArray();
    }

    public EventInfo? GetEvent(string name)
    {
        var evt = symbol.GetMembers()
            .OfType<IEventSymbol>()
            .FirstOrDefault(e => e.Name == name);

        return evt is null ? null : new EventInfo(evt);
    }

    public EventInfo[] GetEvents()
    {
        return symbol.GetMembers()
            .OfType<IEventSymbol>()
            .Select(e => new EventInfo(e))
            .ToArray();
    }

    public ConstructorInfo[] GetConstructors()
    {
        return symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor)
            .Select(c => new ConstructorInfo(c))
            .ToArray();
    }

    public Type[] GetNestedTypes()
    {
        return symbol.GetTypeMembers()
            .Select(t => new Type(t))
            .ToArray();
    }

    public Type? BaseType
    {
        get
        {
            if (symbol is INamedTypeSymbol named &&
                named.BaseType is not null)
            {
                return new Type(named.BaseType);
            }

            return null;
        }
    }

    public Type[] GetInterfaces()
    {
        if (symbol is not INamedTypeSymbol named)
            return [];

        return named.Interfaces
            .Select(i => new Type(i))
            .ToArray();
    }

    public Type[] GetGenericArguments()
    {
        if (symbol is not INamedTypeSymbol named)
            return [];

        return named.TypeArguments
            .Select(t => new Type(t))
            .ToArray();
    }

    public AttributeInfo[] GetCustomAttributes()
    {
        return symbol.GetAttributes()
            .Select(a => new AttributeInfo(a))
            .ToArray();
    }

    public bool IsClass =>
        symbol.TypeKind == TypeKind.Class;

    public bool IsStruct =>
        symbol.TypeKind == TypeKind.Struct;

    public bool IsInterface =>
        symbol.TypeKind == TypeKind.Interface;

    public bool IsEnum =>
        symbol.TypeKind == TypeKind.Enum;

    public bool IsGenericType =>
        symbol is INamedTypeSymbol { IsGenericType: true };

    public bool IsAbstract =>
        symbol.IsAbstract;

    public bool IsSealed =>
        symbol.IsSealed;

    public bool IsStatic =>
        symbol.IsStatic;

    public override string ToString() => FullName;

    internal ITypeSymbol Symbol => symbol;
}