namespace Reflection;

[Flags]
public enum BindingFlags
{
    None = 0,
    Instance = 1,
    Static = 2,
    Public = 4,
    NonPublic = 8
}