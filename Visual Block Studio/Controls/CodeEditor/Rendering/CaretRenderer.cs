using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class CaretRenderer
{
    public void Render(CanvasDrawingSession drawingSession, EditorEngine engine, EditorTextStyle style)
    {
        if (!engine.Caret.IsVisible)
            return;

        float x = 10 + (engine.Caret.Column * style.CharacterWidth);

        float caretHeight = style.LineHeight * 0.9f;
        float caretOffset = (style.LineHeight - caretHeight) / 2;

        float y = (engine.Caret.Line * style.LineHeight) + caretOffset;

        drawingSession.DrawLine(
            x,
            y,
            x,
            y + caretHeight,
            Colors.White,
            1);
    }
}