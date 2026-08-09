using Microsoft.CodeAnalysis;
using System.Globalization;

namespace Reflection;

public abstract class MemberInfo
{
    public abstract string Name { get; }

    public abstract object[] GetCustomAttributes(bool inherit);

    public abstract bool IsDefined(Type attributeType, bool inherit);
}

public abstract class MethodBase : MemberInfo
{
    public abstract ParameterInfo[] GetParameters();
}

public sealed class MethodInfo(IMethodSymbol symbol) : MethodBase
{
    public override string Name => symbol.Name;

    public Type ReturnType =>
        new Type(symbol.ReturnType);

    public AttributeInfo[] ReturnTypeCustomAttributes =>
        symbol.GetReturnTypeAttributes()
              .Select(a => new AttributeInfo(a))
              .ToArray();

    public MethodAttributes Attributes
    {
        get
        {
            MethodAttributes attributes = MethodAttributes.None;

            if (symbol.IsStatic)
                attributes |= MethodAttributes.Static;

            if (symbol.IsAbstract)
                attributes |= MethodAttributes.Abstract;

            if (symbol.IsVirtual)
                attributes |= MethodAttributes.Virtual;

            if (symbol.DeclaredAccessibility == Accessibility.Public)
                attributes |= MethodAttributes.Public;

            if (symbol.DeclaredAccessibility == Accessibility.Private)
                attributes |= MethodAttributes.Private;

            return attributes;
        }
    }

    public Type? DeclaringType =>
        symbol.ContainingType is null
            ? null
            : new Type(symbol.ContainingType);

    public Type? ReflectedType => DeclaringType;

    public MethodInfo GetBaseDefinition()
    {
        var overridden = symbol.OverriddenMethod;

        return overridden is null
            ? this
            : new MethodInfo(overridden);
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return symbol.GetAttributes()
            .Select(a => new AttributeInfo(a))
            .ToArray();
    }

    public object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return symbol.GetAttributes()
            .Where(a =>
                a.AttributeClass?.ToDisplayString() ==
                attributeType.FullName)
            .Select(a => new AttributeInfo(a))
            .ToArray();
    }

    public MethodImplAttributes GetMethodImplementationFlags()
    {
        return MethodImplAttributes.None;
    }

    public override ParameterInfo[] GetParameters()
    {
        return symbol.Parameters
            .Select(p => new ParameterInfo(p))
            .ToArray();
    }

    public object? Invoke(
        object? obj,
        BindingFlags invokeAttr,
        Binder? binder,
        object?[]? parameters,
        CultureInfo? culture)
    {
        throw new NotSupportedException(
            "Invocation is not yet supported by the Roslyn reflection provider.");
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return symbol.GetAttributes()
            .Any(a =>
                a.AttributeClass?.ToDisplayString() ==
                attributeType.FullName);
    }
}