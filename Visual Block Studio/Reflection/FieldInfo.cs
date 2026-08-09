using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class FieldInfo(IFieldSymbol symbol) : MemberInfo
{
    public override string Name => symbol.Name;

    public Type FieldType =>
        new Type(symbol.Type);

    public bool IsStatic =>
        symbol.IsStatic;

    public bool IsReadOnly =>
        symbol.IsReadOnly;

    public override object[] GetCustomAttributes(bool inherit)
    {
        throw new NotImplementedException();
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        throw new NotImplementedException();
    }
}