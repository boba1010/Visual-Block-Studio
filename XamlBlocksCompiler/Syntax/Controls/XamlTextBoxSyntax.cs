namespace XamlBlocksCompiler.Syntax.Controls;

[XamlControl]
public sealed class XamlTextBoxSyntax() : XamlControlSyntax("TextBox")
{
    public string Text { get; set; } = null!;
    public double FontSize { get; set; } = 16;
}
