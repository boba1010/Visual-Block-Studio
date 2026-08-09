using System.Numerics;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Editing;

public sealed class VBlockEditor
{
    public XamlBlock? Root { get; private set; }

    public BlockSelection Selection { get; } = new();

    private XamlBlock? _draggedBlock;
    private Vector2 _lastMousePosition;

    public void SetRoot(XamlBlock? root)
    {
        Root = root;

        _draggedBlock = null;
        Selection.Clear();
    }

    public void PointerPressed(Vector2 position)
    {
        _draggedBlock = null;

        if (Root is null)
        {
            Selection.Clear();
            return;
        }

        var block = FindBlockAt(
            Root,
            position);

        if (block is null)
        {
            Selection.Clear();
            return;
        }

        _draggedBlock = block;
        _lastMousePosition = position;

        Selection.Select(block);
    }

    public bool PointerMoved(Vector2 position)
    {
        if (_draggedBlock is null)
            return false;

        Vector2 delta =
            position - _lastMousePosition;

        _draggedBlock.Translate(delta);

        _lastMousePosition = position;

        return true;
    }

    public void PointerReleased()
    {
        _draggedBlock = null;
    }

    private static XamlBlock? FindBlockAt(
        XamlBlock current,
        Vector2 point)
    {
        // Children are rendered above their parent,
        // so test them first.
        for (int i = current.Children.Count - 1; i >= 0; i--)
        {
            var child = current.Children[i];

            var found = FindBlockAt(
                child,
                point);

            if (found is not null)
                return found;
        }

        var bounds = new Rect(
            current.Position.X,
            current.Position.Y,
            current.Size.X,
            current.Size.Y);

        return bounds.Contains(
            new Point(
                point.X,
                point.Y))
            ? current
            : null;
    }
}