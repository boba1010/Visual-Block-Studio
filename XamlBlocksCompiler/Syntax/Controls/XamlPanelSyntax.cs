using XamlBlocksCompiler.Collections;
using XamlBlocksCompiler.Models;

namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlPanelSyntax : XamlControlSyntax
{
    public XamlElementCollection Children { get; }
    public Color Background { get; set; } = Color.Transparent;

    public XamlPanelSyntax(string name) : base(name)
    {
        Children = new(this);
    }
}
