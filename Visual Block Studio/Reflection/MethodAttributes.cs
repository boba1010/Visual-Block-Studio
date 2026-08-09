namespace Reflection;

[Flags]
public enum MethodAttributes
{
    None = 0,
    Public = 1,
    Private = 2,
    Static = 4,
    Abstract = 8,
    Virtual = 16
}

public enum MethodImplAttributes
{
    None = 0
}