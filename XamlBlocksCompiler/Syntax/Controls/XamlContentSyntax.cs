namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlContentSyntax(string name) : XamlControlSyntax(name)
{
    public object? Content { get; set; }
}
