using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public sealed class XamlDesignerDocument
{
    private readonly Dictionary<FrameworkElement, XamlElementSyntax> _syntaxByElement = [];
    private readonly List<FrameworkElement> _elements = [];

    public int SyntaxCount => _syntaxByElement.Count;
    public XamlElementSyntax? RootSyntax { get; set; }

    public void AddSyntaxRelation(FrameworkElement element, XamlElementSyntax syntax)
    {
        _syntaxByElement.Add(element, syntax);
        _elements.Add(element);
    }

    public void RemoveSyntaxRelation(FrameworkElement element)
    {
        if (!_syntaxByElement.Remove(element, out var syntax))
            return;

        if (ReferenceEquals(syntax, RootSyntax))
            RootSyntax = null;
        else if (RootSyntax is XamlPanelSyntax root)
            DetachSyntaxNode(root, syntax);

        _elements.Remove(element);
    }

    private static bool DetachSyntaxNode(XamlPanelSyntax parent, XamlElementSyntax target)
    {
        if (parent.Children.Remove(target))
            return true;

        foreach (var child in parent.Children)
        {
            if (child is XamlPanelSyntax panel && DetachSyntaxNode(panel, target))
                return true;
        }

        return false;
    }

    public void RemoveSyntaxRelations(IEnumerable<FrameworkElement> elements)
    {
        foreach (var element in elements)
            RemoveSyntaxRelation(element);
    }

    public bool TryGetSyntax(FrameworkElement element, out XamlElementSyntax? syntax)
    {
        return _syntaxByElement.TryGetValue(element, out syntax);
    }

    public FrameworkElement ElementAt(int index) => _elements[index];
}
