using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class NewLineEdit(
    TextPosition beforeCaret,
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection) : IUndoableEdit
{
    public void Undo(EditorEngine editor)
    {
        editor.Document.SetLineText(
            beforeCaret.Line,
            editor.Document[beforeCaret.Line].Text +
            editor.Document[beforeCaret.Line + 1].Text);

        editor.Document.RemoveLine(beforeCaret.Line + 1);

        editor.Caret.Position = beforeCaret;
        editor.Selection.Restore(beforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        string text = editor.Document[beforeCaret.Line].Text;

        string before = text[..beforeCaret.Column];
        string after = text[beforeCaret.Column..];

        editor.Document.SetLineText(beforeCaret.Line, before);
        editor.Document.InsertLine(beforeCaret.Line + 1, new DocumentLine(after));

        editor.Caret.Position = afterCaret;
        editor.Selection.Restore(afterSelection);
    }
}
