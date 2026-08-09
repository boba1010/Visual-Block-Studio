using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class PropertyInfo(IPropertySymbol symbol) : MemberInfo
{
    public override string Name => symbol.Name;

    public Type PropertyType => new(symbol.Type);

    public bool CanRead =>
        symbol.GetMethod is not null;

    public bool CanWrite =>
        symbol.SetMethod is not null;

    public override object[] GetCustomAttributes(bool inherit)
    {
        throw new NotImplementedException();
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        throw new NotImplementedException();
    }
}