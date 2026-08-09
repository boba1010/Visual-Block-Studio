using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class TextRenderer
{
    public void Render(CanvasDrawingSession drawingSession, EditorEngine engine, EditorTextStyle style)
    {
        float y = 0;

        foreach (DocumentLine line in engine.Document.Lines)
        {
            drawingSession.DrawText(
                line.Text,
                10,
                y,
                Colors.White,
                style.CreateTextFormat());

            y += style.LineHeight;
        }
    }
}