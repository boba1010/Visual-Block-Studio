using XamlBlocksCompiler.Models;

namespace XamlBlocksCompiler.Syntax.Controls;

public abstract class XamlControlSyntax(string name) : XamlElementSyntax(name)
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Color Background { get; set; } = Color.Transparent;

    public Alignment VerticalAlignment { get; set; } = Alignment.Center;
    public Alignment HorizontalAlignment { get; set; } = Alignment.Center;
}
