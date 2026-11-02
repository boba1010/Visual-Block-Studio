using XamlBlocksCompiler.Models;

namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlPanelSyntax(string name) : XamlControlSyntax(name)
{
    public List<XamlElementSyntax> Children { get; } = [];
    public Color Background { get; set; } = Color.Transparent;
}
