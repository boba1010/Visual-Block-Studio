using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class DeleteTextEdit(
    TextPosition start,
    string text,
    TextPosition beforeCaret,
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection) : IUndoableEdit
{
    public void Undo(EditorEngine editor)
    {
        editor.Document.InsertText(
            start.Line,
            start.Column,
            text);

        editor.Caret.Position = beforeCaret;
        editor.Selection.Restore(beforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        editor.Document.RemoveText(
            start.Line,
            start.Column,
            text);

        editor.Caret.Position = afterCaret;
        editor.Selection.Restore(afterSelection);
    }
}
