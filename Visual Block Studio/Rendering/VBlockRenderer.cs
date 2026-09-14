using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using System.Numerics;
using Visual_Block_Studio.Editing;
using Visual_Block_Studio.Models.XamlBlocks;

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
    private const float FlyoutWidth = 240f;
    private const float FlyoutPadding = 12f;
    private const float FlyoutOffset = 8f;
    private const float PropertyHeight = 30f;

    public void Render(CanvasDrawingSession ds, VBlockEditor editor)
    {
        if (editor.Root is null)
            return;

        ds.Transform = Matrix3x2.CreateScale(editor.Zoom) * Matrix3x2.CreateTranslation(editor.Scroll);

        RenderBlock(ds, editor.Root, editor.Selection, editor);

        if (editor.OpenPropertyFlyout is not null)
            DrawPropertyFlyout(ds, editor.OpenPropertyFlyout);

        ds.Transform = Matrix3x2.Identity;
    }

    private void RenderBlock(CanvasDrawingSession ds, XamlBlock block, BlockSelection selection, VBlockEditor editor)
    {
        var rect = GetVisualBounds(block);

        ds.FillRoundedRectangle(rect, CornerRadius, CornerRadius, _backgroundBrush);
        ds.DrawRoundedRectangle(rect, CornerRadius, CornerRadius, _borderBrush, 1f);

        var headerRect = new Rect(block.Position.X, block.Position.Y, block.Size.X, HeaderHeight);
        var textRect = new Rect(headerRect.X + Padding, headerRect.Y + Padding, Math.Max(0, headerRect.Width - Padding * 2 - ArrowSize * 2 - Padding * 2), HeaderHeight);

        ds.DrawText(block.GetType().Name, textRect, _textBrush, _textFormat);
        DrawPropertyArrow(ds, block, editor);

        if (selection.IsSelected(block))
            ds.DrawRoundedRectangle(rect, CornerRadius, CornerRadius, _borderBrush, 2f);

        foreach (var child in block.Children)
            RenderBlock(ds, child, selection, editor);
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

    private void DrawPropertyArrow(CanvasDrawingSession ds, XamlBlock block, VBlockEditor editor)
    {
        float centerX = block.Position.X + block.Size.X - Padding - ArrowSize / 2f;
        float centerY = block.Position.Y + Padding + ArrowSize / 2f;
        float angle = block == editor.OpenPropertyFlyout ? 90f : 0f;

        var center = new Vector2(centerX, centerY);
        var left = new Vector2(centerX - ArrowSize / 2f, centerY - ArrowSize / 4f);
        var middle = new Vector2(centerX, centerY + ArrowSize / 4f);
        var right = new Vector2(centerX + ArrowSize / 2f, centerY - ArrowSize / 4f);

        left = Rotate(left, center, angle);
        middle = Rotate(middle, center, angle);
        right = Rotate(right, center, angle);

        ds.DrawLine(left, middle, _textBrush, ArrowStrokeWidth);
        ds.DrawLine(middle, right, _textBrush, ArrowStrokeWidth);
    }

    private static Vector2 Rotate(Vector2 point, Vector2 center, float degrees)
    {
        float radians = degrees * MathF.PI / 180f;
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        var p = point - center;

        return new Vector2(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos) + center;
    }

    private void DrawPropertyFlyout(CanvasDrawingSession ds, XamlBlock block)
    {
        var blockRect = GetVisualBounds(block);
        float flyoutX = (float)blockRect.Right + FlyoutOffset;
        float flyoutY = (float)blockRect.Top;

        var flyoutRect = new Rect(flyoutX, flyoutY, FlyoutWidth, 60f);

        ds.FillRoundedRectangle(flyoutRect, CornerRadius, CornerRadius, _backgroundBrush);
        ds.DrawRoundedRectangle(flyoutRect, CornerRadius, CornerRadius, _borderBrush, 1f);

        var headerRect = new Rect(flyoutRect.X + FlyoutPadding, flyoutRect.Y + FlyoutPadding, flyoutRect.Width - FlyoutPadding * 2, PropertyHeight);
        ds.DrawText("Properties", headerRect, _textBrush, _textFormat);
    }
}