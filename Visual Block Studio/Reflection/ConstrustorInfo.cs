using Microsoft.CodeAnalysis;

namespace Reflection;

public sealed class ConstructorInfo(IMethodSymbol symbol) : MethodBase
{
    public override string Name => symbol.Name;

    public override object[] GetCustomAttributes(bool inherit)
    {
        throw new NotImplementedException();
    }

    public override ParameterInfo[] GetParameters()
    {
        return symbol.Parameters
            .Select(p => new ParameterInfo(p))
            .ToArray();
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        throw new NotImplementedException();
    }
}