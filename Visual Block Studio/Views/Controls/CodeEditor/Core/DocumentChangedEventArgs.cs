namespace Visual_Block_Studio.Views.Controls.CodeEditor.Core;

public sealed class DocumentChangedEventArgs(DocumentChangeKind kind, int line) : EventArgs
{
    public DocumentChangeKind Kind { get; } = kind;

    public int Line { get; } = line;
}