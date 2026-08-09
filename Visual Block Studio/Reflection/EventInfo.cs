using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class EventInfo(IEventSymbol symbol) : MemberInfo
{
    public override string Name => symbol.Name;

    public Type EventHandlerType => new(symbol.Type);

    public override object[] GetCustomAttributes(bool inherit)
    {
        throw new NotImplementedException();
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        throw new NotImplementedException();
    }
}