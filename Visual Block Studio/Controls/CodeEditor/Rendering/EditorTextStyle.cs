using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class EditorTextStyle
{
    public string FontFamily { get; init; } = "Cascadia Mono";

    public float FontSize { get; init; } = 14;

    public float CharacterWidth { get; private set; }
    public float LineHeight { get; private set; }
    public float Baseline { get; private set; }

    public CanvasTextFormat CreateTextFormat()
    {
        return new CanvasTextFormat
        {
            FontFamily = FontFamily,
            FontSize = FontSize
        };
    }

    public bool IsMeasured { get; private set; }

    public void Measure(CanvasDrawingSession drawingSession)
    {
        using CanvasTextLayout layout = new(
            drawingSession,
            "M",
            CreateTextFormat(),
            100,
            100);

        CharacterWidth = (float)layout.LayoutBounds.Width;

        var metrics = layout.LineMetrics[0];

        LineHeight = metrics.Height;
        Baseline = metrics.Baseline;

        IsMeasured = true;
    }
}