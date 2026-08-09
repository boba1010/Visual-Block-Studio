using System.Collections.ObjectModel;

namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public sealed class TextDocument
{
    private readonly ObservableCollection<DocumentLine> _lines = [];

    public ReadOnlyObservableCollection<DocumentLine> Lines { get; }

    public int LineCount => _lines.Count;

    public DocumentLine this[int index] => _lines[index];

    public event EventHandler<DocumentChangedEventArgs>? Changed;

    public TextDocument()
    {
        Lines = new ReadOnlyObservableCollection<DocumentLine>(_lines);
        _lines.Add(new DocumentLine());
    }

    public TextDocument(string text)
    {
        Lines = new ReadOnlyObservableCollection<DocumentLine>(_lines);

        Load(text);
    }

    public void Load(string text)
    {
        _lines.Clear();

        text ??= string.Empty;

        string[] lines = text.Replace("\r\n", "\n")
                             .Replace('\r', '\n')
                             .Split('\n');

        foreach (string line in lines)
            _lines.Add(new DocumentLine(line));

        RaiseChanged(DocumentChangeKind.Reset);
    }

    public string GetText()
    {
        return string.Join(Environment.NewLine, _lines.Select(x => x.Text));
    }

    internal void InsertLine(int index, DocumentLine line)
    {
        _lines.Insert(index, line);

        RaiseChanged(DocumentChangeKind.LineInserted, index);
    }

    public void InsertText(int line, int column, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        text = text.Replace("\r\n", "\n")
                   .Replace('\r', '\n');

        string[] lines = text.Split('\n');

        DocumentLine current = _lines[line];

        if (lines.Length == 1)
        {
            if (string.IsNullOrEmpty(current.Text))
            {
                current.Text = lines[0];
            }
            else
            {
                current.Text = current.Text.Insert(column, lines[0]);
            }

            RaiseChanged(
                DocumentChangeKind.LineChanged,
                line);

            return;
        }

        string before = current.Text[..column];
        string after = current.Text[column..];

        current.Text = before + lines[0];

        int insertIndex = line + 1;

        for (int i = 1; i < lines.Length; i++)
        {
            string value = lines[i];

            if (i == lines.Length - 1)
                value += after;

            _lines.Insert(
                insertIndex++,
                new DocumentLine(value));
        }

        RaiseChanged(
            DocumentChangeKind.Reset);
    }

    internal void RemoveLine(int index)
    {
        _lines.RemoveAt(index);

        if (_lines.Count == 0)
            _lines.Add(new DocumentLine());

        RaiseChanged(DocumentChangeKind.LineRemoved, index);
    }

    public void RemoveText(int line, int column, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        text = text.Replace("\r\n", "\n")
                   .Replace('\r', '\n');

        string[] lines = text.Split('\n');

        if (lines.Length == 1)
        {
            string current = _lines[line].Text;

            int safeCount = Math.Min(lines[0].Length, current.Length - column);

            if (column <= current.Length && safeCount > 0)
            {
                _lines[line].Text = current.Remove(column, safeCount);
            }

            RaiseChanged(DocumentChangeKind.LineChanged, line);
            return;
        }


        string first =
            _lines[line].Text[..column];

        string last =
            _lines[line + lines.Length - 1]
                .Text[lines[^1].Length..];

        _lines[line].Text = first + last;

        for (int i = line + lines.Length - 1; i > line; i--)
            RemoveLine(i);

        RaiseChanged(
            DocumentChangeKind.Reset);
    }

    internal void ReplaceLine(int index, DocumentLine line)
    {
        _lines[index] = line;

        RaiseChanged(DocumentChangeKind.LineChanged, index);
    }

    internal void ReplaceText(TextPosition start, string oldText, string newText)
    {
        RemoveText(
            start.Line,
            start.Column,
            oldText);

        InsertText(
            start.Line,
            start.Column,
            newText);
    }

    internal void SetLineText(int index, string text)
    {
        if (_lines[index].Text == text)
            return;

        _lines[index].Text = text;

        RaiseChanged(DocumentChangeKind.LineChanged, index);
    }

    private void RaiseChanged(DocumentChangeKind kind, int line = -1)
    {
        Changed?.Invoke(this, new DocumentChangedEventArgs(kind, line));
    }
}