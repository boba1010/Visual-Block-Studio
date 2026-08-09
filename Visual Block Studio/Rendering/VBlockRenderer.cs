using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using System.Numerics;
using Visual_Block_Studio.Editing;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Rendering;

public sealed class VBlockRenderer(CanvasDevice device)
{
    private readonly CanvasSolidColorBrush _backgroundBrush = new(device, Colors.DimGray);
    private readonly CanvasSolidColorBrush _borderBrush = new(device, Colors.White);
    private readonly CanvasSolidColorBrush _textBrush = new(device, Colors.White);

    private readonly CanvasTextFormat _textFormat = new()
    {
        FontSize = 13,
        HorizontalAlignment = CanvasHorizontalAlignment.Left,
        VerticalAlignment = CanvasVerticalAlignment.Top
    };

    private const float CornerRadius = 8f;
    private const float Padding = 10f;
    private const float HeaderHeight = 28f;
    private const float ArrowSize = 10f;
    private const float ArrowStrokeWidth = 1.5f;

    public void Render(CanvasDrawingSession ds, VBlockEditor editor)
    {
        if (editor.Root is null)
            return;

        RenderBlock(ds, editor.Root, editor.Selection);
    }

    private void RenderBlock(CanvasDrawingSession ds, XamlBlock block, BlockSelection selection)
    {
        var rect = GetVisualBounds(block);

        ds.FillRoundedRectangle(rect, CornerRadius, CornerRadius, _backgroundBrush);
        ds.DrawRoundedRectangle(rect, CornerRadius, CornerRadius, _borderBrush, 1f);

        var textRect = new Rect(
            rect.X + Padding,
            rect.Y + Padding,
            Math.Max(0, rect.Width - Padding * 2 - ArrowSize - Padding),
            HeaderHeight);

        ds.DrawText(block.GetType().Name, textRect, _textBrush, _textFormat);
        DrawPropertyArrow(ds, rect);

        if (selection.IsSelected(block))
            ds.DrawRoundedRectangle(rect, CornerRadius, CornerRadius, _borderBrush, 2f);

        foreach (var child in block.Children)
            RenderBlock(ds, child, selection);
    }

    private static Rect GetVisualBounds(XamlBlock block)
    {
        float left = block.Position.X;
        float top = block.Position.Y;
        float right = block.Position.X + block.Size.X;
        float bottom = block.Position.Y + block.Size.Y;

        foreach (var child in block.Children)
        {
            var childBounds = GetVisualBounds(child);

            left = Math.Min(left, (float)childBounds.Left - Padding);
            top = Math.Min(top, (float)childBounds.Top - HeaderHeight - Padding);
            right = Math.Max(right, (float)childBounds.Right + Padding);
            bottom = Math.Max(bottom, (float)childBounds.Bottom + Padding);
        }

        return new Rect(left, top, right - left, bottom - top);
    }

    private void DrawPropertyArrow(CanvasDrawingSession ds, Rect rect)
    {
        float arrowX = (float)rect.Right - Padding - ArrowSize;
        float arrowY = (float)rect.Y + Padding + 7f;

        var left = new Vector2(arrowX, arrowY);
        var middle = new Vector2(arrowX + ArrowSize / 2f, arrowY + ArrowSize / 2f);
        var right = new Vector2(arrowX + ArrowSize, arrowY);

        ds.DrawLine(left, middle, _textBrush, ArrowStrokeWidth);
        ds.DrawLine(middle, right, _textBrush, ArrowStrokeWidth);
    }
}