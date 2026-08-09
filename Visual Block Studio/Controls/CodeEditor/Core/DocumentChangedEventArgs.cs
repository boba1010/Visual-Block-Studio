namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public sealed class DocumentChangedEventArgs : EventArgs
{
    public DocumentChangedEventArgs(DocumentChangeKind kind, int line)
    {
        Kind = kind;
        Line = line;
    }

    public DocumentChangeKind Kind { get; }

    public int Line { get; }
}