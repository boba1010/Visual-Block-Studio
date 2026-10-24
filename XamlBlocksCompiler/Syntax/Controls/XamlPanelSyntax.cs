namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlPanelSyntax(string name) : XamlControlSyntax(name)
{
    public List<XamlElementSyntax> Children { get; } = [];
}
