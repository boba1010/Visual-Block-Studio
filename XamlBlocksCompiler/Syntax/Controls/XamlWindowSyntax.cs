namespace XamlBlocksCompiler.Syntax.Controls;

[XamlControl]
public sealed class XamlWindowSyntax : XamlElementSyntax
{
    public string Title { get; set; } = "Untitled";
    public object? Content { get; set; }

    public XamlWindowSyntax() : base("Window")
    {
    }
}