using System.Numerics;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Editing;

public sealed class VBlockEditor
{
    public XamlBlock? Root { get; private set; }
    public XamlBlock? OpenPropertyFlyout { get; set; }
    public BlockSelection Selection { get; } = new();
    public float Zoom { get; private set; } = 1f;
    public Vector2 Scroll { get; private set; } = Vector2.Zero;

    private XamlBlock? _draggedBlock;
    private Vector2 _lastMousePosition;
    private bool _isPanning;

    public void SetRoot(XamlBlock? root)
    {
        Root = root;
        OpenPropertyFlyout = null;
        _draggedBlock = null;
        _isPanning = false;
        Selection.Clear();
    }

    public XamlBlock? PointerPressed(Vector2 screenPosition)
    {
        _draggedBlock = null;
        _isPanning = false;

        if (Root is null)
        {
            Selection.Clear();
            _isPanning = true;
            _lastMousePosition = screenPosition;
            return null;
        }

        Vector2 position = ScreenToWorld(screenPosition);

        if (OpenPropertyFlyout is not null && IsPropertyFlyoutHit(OpenPropertyFlyout, position))
        {
            _lastMousePosition = screenPosition;
            return OpenPropertyFlyout;
        }

        var block = FindBlockAt(Root, position);

        if (block is null)
        {
            Selection.Clear();
            OpenPropertyFlyout = null;
            _isPanning = true;
            _lastMousePosition = screenPosition;
            return null;
        }

        if (IsPropertyArrowHit(block, position))
        {
            OpenPropertyFlyout = OpenPropertyFlyout == block ? null : block;
            return block;
        }

        OpenPropertyFlyout = null;
        _draggedBlock = block;
        _lastMousePosition = position;
        Selection.Select(block);

        return null;
    }

    public bool PointerMoved(Vector2 screenPosition)
    {
        if (_isPanning)
        {
            Vector2 panDelta = screenPosition - _lastMousePosition;
            Scroll += panDelta;
            _lastMousePosition = screenPosition;
            return true;
        }

        if (_draggedBlock is null || Root is null)
            return false;

        Vector2 position = ScreenToWorld(screenPosition);
        Vector2 blockDelta = position - _lastMousePosition;

        if (ReferenceEquals(_draggedBlock, Root))
        {
            _draggedBlock.Translate(blockDelta);
        }
        else
        {
            _draggedBlock.Position += blockDelta;

            var parent = FindParent(Root, _draggedBlock);

            if (parent is not null)
                ConstrainToParent(_draggedBlock, parent);
        }

        _lastMousePosition = position;
        return true;
    }

    public void PointerReleased()
    {
        _draggedBlock = null;
        _isPanning = false;
    }

    private static XamlBlock? FindBlockAt(XamlBlock current, Vector2 point)
    {
        for (int i = current.Children.Count - 1; i >= 0; i--)
        {
            var child = current.Children[i];
            var found = FindBlockAt(child, point);

            if (found is not null)
                return found;
        }

        var bounds = new Rect(
            current.Position.X,
            current.Position.Y,
            current.Size.X,
            current.Size.Y);

        return bounds.Contains(new Point(point.X, point.Y)) ? current : null;
    }

    private static bool IsPropertyArrowHit(XamlBlock block, Vector2 position)
    {
        const float padding = 10f;
        const float arrowSize = 10f;

        float left = block.Position.X + block.Size.X - padding - arrowSize;
        float top = block.Position.Y + padding;

        return new Rect(left, top, arrowSize, arrowSize)
            .Contains(new Point(position.X, position.Y));
    }

    private static bool IsPropertyFlyoutHit(XamlBlock block, Vector2 position)
    {
        const float flyoutWidth = 240f;
        const float flyoutHeight = 60f;
        const float flyoutOffset = 8f;
        const float padding = 10f;
        const float headerHeight = 28f;

        var bounds = GetVisualBounds(block, padding, headerHeight);

        float left = (float)bounds.Right + flyoutOffset;
        float top = (float)bounds.Top;

        return new Rect(
            left,
            top,
            flyoutWidth,
            flyoutHeight)
            .Contains(new Point(position.X, position.Y));
    }

    private static Rect GetVisualBounds(XamlBlock block, float padding, float headerHeight)
    {
        float left = block.Position.X;
        float top = block.Position.Y;
        float right = block.Position.X + block.Size.X;
        float bottom = block.Position.Y + block.Size.Y;

        foreach (var child in block.Children)
        {
            var childBounds = GetVisualBounds(child, padding, headerHeight);

            left = Math.Min(left, (float)childBounds.Left - padding);
            top = Math.Min(top, (float)childBounds.Top - headerHeight - padding);
            right = Math.Max(right, (float)childBounds.Right + padding);
            bottom = Math.Max(bottom, (float)childBounds.Bottom + padding);
        }

        return new Rect(left, top, right - left, bottom - top);
    }

    private static void ConstrainToParent(XamlBlock block, XamlBlock parent)
    {
        const float padding = 10f;
        const float headerHeight = 28f;

        float minX = parent.Position.X + padding;
        float minY = parent.Position.Y + headerHeight + padding;
        float maxX = parent.Position.X + parent.Size.X - block.Size.X - padding;
        float maxY = parent.Position.Y + parent.Size.Y - block.Size.Y - padding;

        block.Position = new Vector2(
            Math.Clamp(block.Position.X, minX, maxX),
            Math.Clamp(block.Position.Y, minY, maxY));
    }

    private static XamlBlock? FindParent(XamlBlock current, XamlBlock target)
    {
        foreach (var child in current.Children)
        {
            if (ReferenceEquals(child, target))
                return current;

            var parent = FindParent(child, target);

            if (parent is not null)
                return parent;
        }

        return null;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return (screenPosition - Scroll) / Zoom;
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return worldPosition * Zoom + Scroll;
    }

    public void ZoomAt(Vector2 screenPosition, float zoomFactor)
    {
        Vector2 worldPosition = ScreenToWorld(screenPosition);
        float newZoom = Math.Clamp(Zoom * zoomFactor, 0.1f, 5f);

        if (MathF.Abs(newZoom - Zoom) < 0.0001f)
            return;

        Zoom = newZoom;
        Scroll = screenPosition - worldPosition * Zoom;
    }

    public void ScrollBy(Vector2 delta)
    {
        Scroll += delta;
    }
}