using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public enum ChangeType
{
    Remove,
    Add
}

public sealed class XamlDesignerDocumentEventArgs(ChangeType type, XamlControlSyntax syntax, FrameworkElement element) : EventArgs
{
    public ChangeType Type { get; } = type;
    public XamlControlSyntax Syntax { get; } = syntax;
    public FrameworkElement Element { get; } = element;
}

public sealed class XamlDesignerDocument
{
    private readonly Dictionary<FrameworkElement, XamlControlSyntax> _syntaxByElement = [];
    private readonly List<FrameworkElement> _elements = [];

    public int SyntaxCount => _syntaxByElement.Count;

    public XamlElementSyntax? RootSyntax { get; set; }

    public event EventHandler<XamlDesignerDocumentEventArgs>? DocumentChanged;

    public void AddSyntaxRelation(FrameworkElement element, XamlControlSyntax syntax)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(syntax);

        if (_syntaxByElement.ContainsKey(element))
            throw new InvalidOperationException("The element already has a syntax association.");

        if (_syntaxByElement.Values.Any(existing => ReferenceEquals(existing, syntax)))
            throw new InvalidOperationException("The syntax node is already associated with another element.");

        _syntaxByElement.Add(element, syntax);
        _elements.Add(element);

        DocumentChanged?.Invoke(this, new(ChangeType.Add, syntax, element));
    }

    public void RemoveSyntaxRelation(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (!_syntaxByElement.TryGetValue(element, out var syntax))
            return;

        // Removing the root clears the entire document.
        if (ReferenceEquals(syntax, RootSyntax))
        {
            RootSyntax = null;
            _syntaxByElement.Clear();
            _elements.Clear();

            DocumentChanged?.Invoke(this, new(ChangeType.Remove, syntax, element));
            return;
        }

        // A non-root node must be detachable from the syntax tree.
        if (RootSyntax is not XamlPanelSyntax root || !DetachSyntaxNode(root, syntax))
            return;

        _syntaxByElement.Remove(element);
        _elements.Remove(element);

        DocumentChanged?.Invoke(this, new(ChangeType.Remove, syntax, element));
    }

    private static bool DetachSyntaxNode(XamlPanelSyntax parent, XamlElementSyntax target)
    {
        for (int i = 0; i < parent.Children.Count; i++)
        {
            var child = parent.Children[i];

            if (ReferenceEquals(child, target))
            {
                parent.Children.RemoveAt(i);
                return true;
            }

            if (child is XamlPanelSyntax panel && DetachSyntaxNode(panel, target))
                return true;
        }

        return false;
    }

    public void RemoveSyntaxRelations(IEnumerable<FrameworkElement> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        foreach (var element in elements)
            RemoveSyntaxRelation(element);
    }

    public bool TryGetSyntax(FrameworkElement element, out XamlControlSyntax? syntax)
    {
        return _syntaxByElement.TryGetValue(element, out syntax);
    }

    public FrameworkElement ElementAt(int index) => _elements[index];
}