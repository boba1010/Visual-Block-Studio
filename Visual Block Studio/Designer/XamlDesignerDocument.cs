using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public sealed class XamlDesignerDocument
{
    private readonly Dictionary<FrameworkElement, XamlElementSyntax> _syntaxByElement = [];
    private readonly List<FrameworkElement> _elements = [];

    public int SyntaxCount => _syntaxByElement.Count;
    public XamlControlSyntax? RootSyntax { get; set; }

    public void AddSyntax(FrameworkElement element, XamlElementSyntax syntax)
    {
        _syntaxByElement.Add(element, syntax);
        _elements.Add(element);
    }

    public void RemoveSyntax(FrameworkElement element)
    {
        if (!_syntaxByElement.Remove(element))
            return;

        _elements.Remove(element);
    }

    public bool TryGetSyntax(FrameworkElement element, out XamlElementSyntax? syntax)
    {
        return _syntaxByElement.TryGetValue(element, out syntax);
    }

    public FrameworkElement ElementAt(int index) => _elements[index];
}
