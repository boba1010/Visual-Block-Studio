using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class SelectionRenderer
{
    public void Render(CanvasDrawingSession drawingSession, EditorEngine engine, EditorTextStyle style)
    {
        if (!engine.HasSelection)
            return;

        TextPosition start = engine.Selection.Start;
        TextPosition end = engine.Selection.End;

        if (start.Line == end.Line)
        {
            DrawSelection(
                drawingSession,
                start.Line,
                start.Column,
                end.Column,
                style);

            return;
        }

        // First line
        DrawSelection(
            drawingSession,
            start.Line,
            start.Column,
            engine.Document[start.Line].Text.Length,
            style);

        // Middle lines
        for (int i = start.Line + 1; i < end.Line; i++)
        {
            DrawSelection(
                drawingSession,
                i,
                0,
                engine.Document[i].Text.Length,
                style);
        }

        // Last line
        DrawSelection(
            drawingSession,
            end.Line,
            0,
            end.Column,
            style);
    }

    private void DrawSelection(CanvasDrawingSession drawingSession, int line, int startColumn, int endColumn, EditorTextStyle style)
    {
        if (endColumn <= startColumn)
            return;

        float x = 10 + (startColumn * style.CharacterWidth);
        float width = (endColumn - startColumn) * style.CharacterWidth;
        float y = line * style.LineHeight;

        drawingSession.FillRectangle(
            x,
            y,
            width,
            style.LineHeight,
            Colors.DodgerBlue);
    }
}
