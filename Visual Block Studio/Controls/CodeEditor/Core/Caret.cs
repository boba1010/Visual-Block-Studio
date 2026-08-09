namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public sealed class Caret
{
    public int Line { get; internal set; }

    public int Column { get; internal set; }

    public TextPosition Position
    {
        get => new(Line, Column);
        internal set
        {
            Line = value.Line;
            Column = value.Column;
        }
    }

    public int DesiredColumn { get; internal set; }

    public bool IsVisible { get; internal set; } = true;

    internal void MoveTo(int line, int column)
    {
        Line = line;
        Column = column;
    }
    internal void MoveTo(TextPosition position)
    {
        Line = position.Line;
        Column = position.Column;
    }
}