using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class EditorTextStyleMetrics
{
    public float CharacterWidth { get; }

    public float LineHeight { get; }

    public EditorTextStyleMetrics(
        CanvasDrawingSession drawingSession,
        EditorTextStyle style)
    {
        using CanvasTextLayout layout = new(
            drawingSession,
            "M",
            style.CreateTextFormat(),
            100,
            100);

        CharacterWidth = (float)layout.LayoutBounds.Width;
        LineHeight = (float)layout.LayoutBounds.Height;
    }
}