using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class UndoRedoManager
{
    private readonly Stack<IUndoableEdit> _undo = [];
    private readonly Stack<IUndoableEdit> _redo = [];

    public event EventHandler? StateChanged;

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    internal void Push(IUndoableEdit edit)
    {
        ArgumentNullException.ThrowIfNull(edit);

        _undo.Push(edit);
        _redo.Clear();

        RaiseStateChanged();
    }

    public bool Undo(EditorEngine editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        if (!CanUndo)
            return false;

        var edit = _undo.Peek();

        edit.Undo(editor);

        _undo.Pop();
        _redo.Push(edit);

        RaiseStateChanged();

        return true;
    }

    public bool Redo(EditorEngine editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        if (!CanRedo)
            return false;

        IUndoableEdit edit = _redo.Peek();

        edit.Redo(editor);

        _redo.Pop();
        _undo.Push(edit);

        RaiseStateChanged();

        return true;
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();

        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}