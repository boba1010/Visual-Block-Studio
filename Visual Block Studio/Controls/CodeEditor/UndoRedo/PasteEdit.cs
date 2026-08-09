using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class PasteEdit(
    TextPosition start,
    TextPosition beforeCaret,
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection,
    string replacedText,
    string insertedText) : IUndoableEdit
{
    public void Undo(EditorEngine editor)
    {
        editor.Document.RemoveText(
            start.Line,
            start.Column,
            insertedText);

        if (!string.IsNullOrEmpty(replacedText))
        {
            editor.Document.InsertText(
                start.Line,
                start.Column,
                replacedText);
        }

        editor.Caret.Position = beforeCaret;
        editor.Selection.Restore(beforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        if (!string.IsNullOrEmpty(replacedText))
        {
            editor.Document.RemoveText(
                start.Line,
                start.Column,
                replacedText);
        }

        editor.Document.InsertText(
            start.Line,
            start.Column,
            insertedText);

        editor.Caret.Position = afterCaret;
        editor.Selection.Restore(afterSelection);
    }
}