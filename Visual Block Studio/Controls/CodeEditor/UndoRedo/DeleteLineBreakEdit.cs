using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class DeleteLineBreakEdit(
    TextPosition beforeCaret,
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection)
    : IUndoableEdit
{
    public void Undo(EditorEngine editor)
    {
        string merged = editor.Document[beforeCaret.Line].Text;

        string before = merged[..beforeCaret.Column];
        string after = merged[beforeCaret.Column..];

        editor.Document.SetLineText(beforeCaret.Line, before);
        editor.Document.InsertLine(
            beforeCaret.Line + 1,
            new DocumentLine(after));

        editor.Caret.Position = beforeCaret;
        editor.Selection.Restore(beforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        string merged =
            editor.Document[beforeCaret.Line].Text +
            editor.Document[beforeCaret.Line + 1].Text;

        editor.Document.SetLineText(beforeCaret.Line, merged);
        editor.Document.RemoveLine(beforeCaret.Line + 1);

        editor.Caret.Position = afterCaret;
        editor.Selection.Restore(afterSelection);
    }
}
