using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class ReplaceTextEdit(
    TextPosition start,
    string oldText,
    string newText,
    TextPosition beforeCaret,
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection)
    : IUndoableEdit
{
    public void Undo(EditorEngine editor)
    {
        editor.Document.ReplaceText(
            start,
            newText,
            oldText);

        editor.Caret.Position = beforeCaret;
        editor.Selection.Restore(beforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        editor.Document.ReplaceText(
            start,
            oldText,
            newText);

        editor.Caret.Position = afterCaret;
        editor.Selection.Restore(afterSelection);
    }
}
