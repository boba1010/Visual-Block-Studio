namespace Visual_Block_Studio.Controls.CodeEditor.Core;

public readonly record struct TextPosition(int Line, int Column) : IComparable<TextPosition>
{
    public int CompareTo(TextPosition other)
    {
        if (Line != other.Line)
            return Line.CompareTo(other.Line);

        return Column.CompareTo(other.Column);
    }

    public static bool operator <(TextPosition left, TextPosition right)
        => left.CompareTo(right) < 0;

    public static bool operator <=(TextPosition left, TextPosition right)
        => left.CompareTo(right) <= 0;

    public static bool operator >(TextPosition left, TextPosition right)
        => left.CompareTo(right) > 0;

    public static bool operator >=(TextPosition left, TextPosition right)
        => left.CompareTo(right) >= 0;
}