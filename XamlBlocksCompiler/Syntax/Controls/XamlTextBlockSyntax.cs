using XamlBlocksCompiler.Models;

namespace XamlBlocksCompiler.Syntax.Controls;

[XamlControl]
public sealed class XamlTextBlockSyntax : XamlControlSyntax
{
    public XamlTextBlockSyntax() : base("TextBlock")
    {
        Width = double.NaN;
        Height = double.NaN;
    }

    public string Text { get; set; } = null!;
    public double FontSize { get; set; } = 16;
    public Color Foreground { get; set; } = Color.Create(255, 255, 255, 255);
}
