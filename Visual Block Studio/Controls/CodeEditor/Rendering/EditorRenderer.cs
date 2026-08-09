using Microsoft.Graphics.Canvas;
using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.Rendering;

public sealed class EditorRenderer
{
    private readonly SelectionRenderer _selectionRenderer = new();
    private readonly TextRenderer _textRenderer = new();
    private readonly CaretRenderer _caretRenderer = new();

    public void Render(CanvasDrawingSession drawingSession, EditorEngine engine, EditorTextStyle style)
    {
        _selectionRenderer.Render(drawingSession, engine, style);

        _textRenderer.Render(drawingSession, engine, style);

        _caretRenderer.Render(drawingSession, engine, style);
    }
}