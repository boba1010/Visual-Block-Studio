namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public sealed class Selection
{
    public TextPosition Anchor { get; internal set; }

    public TextPosition Active { get; internal set; }

    public bool IsEmpty => Anchor == Active;

    public TextPosition Start => Anchor <= Active ? Anchor : Active;

    public TextPosition End => Anchor <= Active ? Active : Anchor;

    public void Clear(TextPosition position)
    {
        Anchor = position;
        Active = position;
    }

    public void Clear()
    {
        Anchor = Active;
    }

    public void SetActive(TextPosition position)
    {
        Active = position;
    }

    public void SetActive(int line, int column)
    {
        SetActive(new(line, column));
    }

    public SelectionState Snapshot()
    {
        return new(Anchor, Active);
    }

    public void Restore(SelectionState other)
    {
        Anchor = other.Anchor;
        Active = other.Active;
    }
}