using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public sealed record HistoryEntry(ChangeType Type, FrameworkElement Element, XamlControlSyntax Syntax, int Index);

public sealed class HistoryStack
{
    private readonly XamlDesignerDocument _document;
    private readonly Stack<HistoryEntry> _redoStack = [];
    private readonly Stack<HistoryEntry> _undoStack = [];
    private bool _isReplaying;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public HistoryStack(XamlDesignerDocument document)
    {
        _document = document;
        _document.DocumentChanged += OnDocumentChanged;
    }

    private void OnDocumentChanged(object? sender, XamlDesignerDocumentEventArgs e)
    {
        if (_isReplaying)
            return;

        int index = e.Syntax.Parent is XamlPanelSyntax parent ? parent.Children.IndexOf(e.Syntax) : -1;
        _undoStack.Push(new(e.Type, e.Element, e.Syntax, index));
        _redoStack.Clear();
    }

    public void Redo()
    {
        if (!CanRedo)
            return;

        var entry = _redoStack.Pop();

        _isReplaying = true;

        try
        {
            if (entry.Type == ChangeType.Add)
                _document.AddSyntaxRelation(entry.Element, entry.Syntax);
            else
                _document.RemoveSyntaxRelation(entry.Element);
        }
        finally
        {
            _isReplaying = false;
        }

        _undoStack.Push(entry);
    }

    public void Undo()
    {
        if (!CanUndo)
            return;

        var entry = _undoStack.Pop();

        _isReplaying = true;

        try
        {
            if (entry.Type == ChangeType.Add)
                _document.RemoveSyntaxRelation(entry.Element);
            else
                _document.AddSyntaxRelation(entry.Element, entry.Syntax);
        }
        finally
        {
            _isReplaying = false;
        }

        _redoStack.Push(entry);
    }
}
