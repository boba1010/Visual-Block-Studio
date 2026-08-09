using Visual_Block_Studio.Controls.CodeEditor.UndoRedo;
using Visual_Block_Studio.Helpers;
using System.Text;

namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public sealed class EditorEngine
{
    public TextDocument Document { get; }

    public Caret Caret { get; }

    public Selection Selection { get; }

    public UndoRedoManager UndoRedo { get; }

    public bool HasSelection => Selection.Start != Selection.End;

    public string ClipboardText { get; private set; } = "";

    public event EventHandler<DocumentChangedEventArgs>? DocumentChanged;

    public EditorEngine()
    {
        Document = new TextDocument();
        Caret = new Caret();
        Selection = new Selection();
        UndoRedo = new UndoRedoManager();
    }

    public void Insert(char c)
    {
        Insert(c.ToString());
    }

    private TextPosition CalculatePositionAfterInsert(TextPosition start, string text)
    {
        text = text.Replace("\r\n", "\n")
                   .Replace('\r', '\n');

        string[] lines = text.Split('\n');

        if (lines.Length == 1)
        {
            return new TextPosition(
                start.Line,
                start.Column + text.Length);
        }

        return new TextPosition(
            start.Line + lines.Length - 1,
            lines[^1].Length);
    }

    private void InsertCore(string text)
    {
        // Callers are responsible for clearing any selection first —
        // this only ever performs a plain insert at the caret.
        Document.InsertText(
            Caret.Line,
            Caret.Column,
            text);

        Caret.Position = CalculatePositionAfterInsert(Caret.Position, text);
        Caret.DesiredColumn = Caret.Column;
    }

    public void Insert(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        if (HasSelection)
        {
            TextPosition start = Selection.Start;
            string oldText = GetSelectedText();

            Document.ReplaceText(start, oldText, text);

            Caret.Position = CalculatePositionAfterInsert(start, text);
            Caret.DesiredColumn = Caret.Column;
            Selection.Clear(Caret.Position);

            UndoRedo.Push(
                new ReplaceTextEdit(
                    start,
                    oldText,
                    text,
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        TextPosition insertPosition = beforeCaret;

        InsertCore(text);

        UndoRedo.Push(
            new InsertTextEdit(
                insertPosition.Line,
                insertPosition.Column,
                text,
                beforeCaret,
                Caret.Position,
                beforeSelection,
                Selection.Snapshot()));
    }

    public void InsertTab()
    {
        Insert("    ");
    }

    private string RemoveIndentCore()
    {
        DocumentLine line = Document[Caret.Line];

        int removeCount = 0;

        while (removeCount < line.Text.Length &&
               removeCount < 4 &&
               line.Text[removeCount] == ' ')
        {
            removeCount++;
        }

        if (removeCount == 0)
            return string.Empty;

        string removed = line.Text[..removeCount];

        Document.SetLineText(
            Caret.Line,
            line.Text[removeCount..]);

        Caret.Column = Math.Max(0, Caret.Column - removeCount);
        Caret.DesiredColumn = Caret.Column;

        return removed;
    }

    public void RemoveIndent()
    {
        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        string removed = RemoveIndentCore();
        if (removed.Length == 0)
            return;

        TextPosition afterCaret = Caret.Position;
        SelectionState afterSelection = Selection.Snapshot();

        UndoRedo.Push(
            new DeleteTextEdit(
                new TextPosition(Caret.Line, 0),
                removed,
                beforeCaret,
                afterCaret,
                beforeSelection,
                afterSelection));
    }

    public void NewLine()
    {
        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        if (HasSelection)
        {
            // Splitting the line is equivalent to inserting "\n" at the
            // selection start, so this is a plain replace: reuse
            // Document.ReplaceText / ReplaceTextEdit instead of duplicating
            // the line-split logic here.
            TextPosition start = Selection.Start;
            string oldText = GetSelectedText();

            Document.ReplaceText(start, oldText, "\n");

            Caret.Line = start.Line + 1;
            Caret.Column = 0;
            Caret.DesiredColumn = 0;
            Selection.Clear(Caret.Position);

            UndoRedo.Push(
                new ReplaceTextEdit(
                    start,
                    oldText,
                    "\n",
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        NewLineCore();

        TextPosition afterCaret = Caret.Position;
        SelectionState afterSelection = Selection.Snapshot();

        UndoRedo.Push(
            new NewLineEdit(
                beforeCaret,
                afterCaret,
                beforeSelection,
                afterSelection));
    }

    private void NewLineCore()
    {
        DocumentLine currentLine = Document[Caret.Line];

        string before = currentLine.Text[..Caret.Column];
        string after = currentLine.Text[Caret.Column..];

        Document.SetLineText(Caret.Line, before);
        Document.InsertLine(Caret.Line + 1, new DocumentLine(after));

        Caret.Line++;
        Caret.Column = 0;
        Caret.DesiredColumn = 0;
    }

    private void BackspaceCharacterCore()
    {
        DocumentLine line = Document[Caret.Line];

        Document.SetLineText(
            Caret.Line,
            line.Text.Remove(Caret.Column - 1, 1));

        Caret.Column--;
        Caret.DesiredColumn = Caret.Column;
    }

    private void BackspaceLineBreakCore()
    {
        DocumentLine previous = Document[Caret.Line - 1];
        DocumentLine current = Document[Caret.Line];

        int previousLength = previous.Text.Length;

        Document.SetLineText(
            Caret.Line - 1,
            previous.Text + current.Text);

        Document.RemoveLine(Caret.Line);

        Caret.Line--;
        Caret.Column = previousLength;
        Caret.DesiredColumn = Caret.Column;
    }

    public void Backspace()
    {
        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        if (HasSelection)
        {
            string deleted = GetSelectedText();

            DeleteSelectionCore();

            UndoRedo.Push(
                new DeleteTextEdit(
                    beforeCaret,
                    deleted,
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        if (Caret.Column > 0)
        {
            string deleted = Document[Caret.Line].Text[Caret.Column - 1].ToString();

            BackspaceCharacterCore();

            UndoRedo.Push(
                new DeleteTextEdit(
                    new TextPosition(Caret.Line, Caret.Column),
                    deleted,
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        if (Caret.Line == 0)
            return;

        BackspaceLineBreakCore();

        UndoRedo.Push(
            new DeleteLineBreakEdit(
                beforeCaret,
                Caret.Position,
                beforeSelection,
                Selection.Snapshot()));
    }

    private void DeleteCharacterCore()
    {
        DocumentLine line = Document[Caret.Line];

        Document.SetLineText(
            Caret.Line,
            line.Text.Remove(Caret.Column, 1));
    }

    private void DeleteLineBreakCore()
    {
        DocumentLine line = Document[Caret.Line];
        DocumentLine next = Document[Caret.Line + 1];

        Document.SetLineText(
            Caret.Line,
            line.Text + next.Text);

        Document.RemoveLine(Caret.Line + 1);
    }

    public void Delete()
    {
        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        if (HasSelection)
        {
            TextPosition start = Selection.Start;
            string deleted = GetSelectedText();

            DeleteSelectionCore();

            UndoRedo.Push(
                new DeleteTextEdit(
                    start,
                    deleted,
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        DocumentLine line = Document[Caret.Line];

        if (Caret.Column < line.Text.Length)
        {
            string deleted = line.Text[Caret.Column].ToString();

            DeleteCharacterCore();

            UndoRedo.Push(
                new DeleteTextEdit(
                    beforeCaret,
                    deleted,
                    beforeCaret,
                    Caret.Position,
                    beforeSelection,
                    Selection.Snapshot()));

            return;
        }

        if (Caret.Line >= Document.LineCount - 1)
            return;

        DeleteLineBreakCore();

        UndoRedo.Push(
            new DeleteLineBreakEdit(
                beforeCaret,
                Caret.Position,
                beforeSelection,
                Selection.Snapshot()));
    }

    public void DeleteSelection()
    {
        if (Selection.IsEmpty)
            return;

        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        TextPosition start = Selection.Start;
        string deletedText = GetSelectedText();

        DeleteSelectionCore();

        TextPosition afterCaret = Caret.Position;
        SelectionState afterSelection = Selection.Snapshot();

        UndoRedo.Push(
            new DeleteTextEdit(
                start,
                deletedText,
                beforeCaret,
                afterCaret,
                beforeSelection,
                afterSelection));
    }

    private void DeleteSelectionCore()
    {
        if (Selection.IsEmpty)
            return;

        TextPosition start = Selection.Start;
        TextPosition end = Selection.End;

        if (start.Line == end.Line)
        {
            DocumentLine line = Document[start.Line];

            Document.SetLineText(
                start.Line,
                line.Text.Remove(start.Column, end.Column - start.Column));
        }
        else
        {
            DocumentLine first = Document[start.Line];
            DocumentLine last = Document[end.Line];

            string text =
                first.Text[..start.Column] +
                last.Text[end.Column..];

            Document.SetLineText(start.Line, text);

            for (int i = end.Line; i > start.Line; i--)
                Document.RemoveLine(i);
        }

        Caret.Line = start.Line;
        Caret.Column = start.Column;
        Caret.DesiredColumn = start.Column;

        Selection.Clear(start);
    }

    private void UpdateSelection(bool extendSelection, TextPosition oldPosition)
    {
        TextPosition newPosition = new(Caret.Line, Caret.Column);

        if (extendSelection)
        {
            if (Selection.IsEmpty)
                Selection.Anchor = oldPosition;

            Selection.SetActive(newPosition);
        }
        else
        {
            Selection.Clear(newPosition);
        }
    }

    public void MoveLeft(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        if (Caret.Column > 0)
        {
            Caret.Column--;
        }
        else if (Caret.Line > 0)
        {
            Caret.Line--;
            Caret.Column = Document[Caret.Line].Text.Length;
        }

        Caret.DesiredColumn = Caret.Column;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveRight(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        DocumentLine line = Document[Caret.Line];

        if (Caret.Column < line.Text.Length)
        {
            Caret.Column++;
        }
        else if (Caret.Line < Document.LineCount - 1)
        {
            Caret.Line++;
            Caret.Column = 0;
        }

        Caret.DesiredColumn = Caret.Column;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveUp(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        if (Caret.Line == 0)
            return;

        Caret.Line--;

        Caret.Column = Math.Min(
            Caret.DesiredColumn,
            Document[Caret.Line].Text.Length);

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveDown(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        if (Caret.Line >= Document.LineCount - 1)
            return;

        Caret.Line++;

        Caret.Column = Math.Min(
            Caret.DesiredColumn,
            Document[Caret.Line].Text.Length);

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveHome(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        string text = Document[Caret.Line].Text;

        int firstNonWhitespace = 0;

        while (firstNonWhitespace < text.Length &&
               char.IsWhiteSpace(text[firstNonWhitespace]))
        {
            firstNonWhitespace++;
        }

        if (Caret.Column != firstNonWhitespace)
            Caret.Column = firstNonWhitespace;
        else
            Caret.Column = 0;

        Caret.DesiredColumn = Caret.Column;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveToLineEnd(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        Caret.Column = Document[Caret.Line].Text.Length;
        Caret.DesiredColumn = Caret.Column;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveToDocumentStart(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        Caret.Line = 0;
        Caret.Column = 0;
        Caret.DesiredColumn = 0;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveToDocumentEnd(bool extendSelection = false)
    {
        TextPosition oldPosition = new(Caret.Line, Caret.Column);

        Caret.Line = Document.LineCount - 1;
        Caret.Column = Document[Caret.Line].Text.Length;
        Caret.DesiredColumn = Caret.Column;

        UpdateSelection(extendSelection, oldPosition);
    }

    public void MoveCaretTo(int line, int column, bool isSelecting = false)
    {
        line = Math.Clamp(line, 0, Document.LineCount - 1);
        column = Math.Clamp(column, 0, Document[line].Text.Length);

        var newPosition = new TextPosition(line, column);

        if (isSelecting)
        {
            Selection.SetActive(newPosition);
        }
        else
        {
            Selection.Clear(newPosition);
        }

        Caret.Position = newPosition;
        Caret.DesiredColumn = column;
    }


    public void SelectAll()
    {
        TextPosition start = new(0, 0);

        int lastLine = Document.LineCount - 1;
        TextPosition end = new(lastLine, Document[lastLine].Text.Length);

        Selection.Anchor = start;
        Selection.Active = end;

        Caret.Position = end;
        Caret.DesiredColumn = end.Column;
    }

    public void ClearSelection()
    {
        TextPosition position = new(Caret.Line, Caret.Column);

        Selection.Clear(position);
    }

    public void Copy()
    {
        ClipboardText = GetSelectedText();
        if (!string.IsNullOrEmpty(ClipboardText))
            Clipboard.SetText(ClipboardText);
    }

    public void Cut()
    {
        if (!HasSelection)
            return;

        ClipboardText = GetSelectedText();
        if (!string.IsNullOrEmpty(ClipboardText))
            Clipboard.SetText(ClipboardText);
        DeleteSelection();
    }

    public void Paste()
    {
        ClipboardText = Clipboard.GetText();

        string text = ClipboardText;

        if (string.IsNullOrEmpty(text))
            return;

        TextPosition beforeCaret = Caret.Position;
        SelectionState beforeSelection = Selection.Snapshot();

        TextPosition start = Selection.IsEmpty
            ? Caret.Position
            : Selection.Start;

        string oldText = GetSelectedText();

        if (!Selection.IsEmpty)
            DeleteSelectionCore();

        InsertCore(text);

        TextPosition afterCaret = Caret.Position;
        SelectionState afterSelection = Selection.Snapshot();

        UndoRedo.Push(
            new ReplaceTextEdit(
                start,
                oldText,
                text,
                beforeCaret,
                afterCaret,
                beforeSelection,
                afterSelection));
    }

    public bool Undo()
    {
        return UndoRedo.Undo(this);
    }

    public bool Redo()
    {
        return UndoRedo.Redo(this);
    }

    public string GetSelectedText()
    {
        if (Selection.IsEmpty)
            return string.Empty;

        TextPosition start = Selection.Start;
        TextPosition end = Selection.End;

        if (start.Line == end.Line)
        {
            return Document[start.Line]
                .Text[start.Column..end.Column];
        }

        StringBuilder builder = new();

        builder.Append(Document[start.Line].Text[start.Column..]);
        builder.Append('\n');

        for (int i = start.Line + 1; i < end.Line; i++)
        {
            builder.Append(Document[i].Text);
            builder.Append('\n');
        }

        builder.Append(Document[end.Line].Text[..end.Column]);

        return builder.ToString();
    }
}