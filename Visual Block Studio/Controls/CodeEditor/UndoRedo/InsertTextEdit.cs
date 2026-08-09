using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public sealed class InsertTextEdit(
    int line, int column, string text, 
    TextPosition beforeCaret, 
    TextPosition afterCaret,
    SelectionState beforeSelection,
    SelectionState afterSelection) : IUndoableEdit
{

    public TextPosition BeforeCaret { get; } = beforeCaret;
    public TextPosition AfterCaret { get; } = afterCaret;

    public SelectionState BeforeSelection { get; } = beforeSelection;
    public SelectionState AfterSelection { get; } = afterSelection;

    public int Line { get; } = line;
    public int Column { get; } = column;
    public string Text { get; } = text;

    public void Undo(EditorEngine editor)
    {
        editor.Document.RemoveText(Line, Column, Text);
        editor.Caret.Position = BeforeCaret;
        editor.Selection.Restore(BeforeSelection);
    }

    public void Redo(EditorEngine editor)
    {
        editor.Document.InsertText(Line, Column, Text);
        editor.Caret.Position = AfterCaret;
        editor.Selection.Restore(AfterSelection);
    }
}