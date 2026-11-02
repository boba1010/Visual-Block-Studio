using XamlBlocksCompiler.Models;

namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlContentSyntax(string name) : XamlControlSyntax(name)
{
    public object? Content { get; set; }
    public Color Background { get; set; } = Color.Transparent;
    public Color Foreground { get; set; } = Color.Create(255, 255, 255, 255);
}
