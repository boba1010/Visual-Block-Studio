using Microsoft.UI.Xaml.Shapes;

namespace Visual_Block_Studio.Designer.Selection;

public sealed class SelectionManager(Canvas selectionOverlay, Canvas designSurface, XamlDesignerDocument document)
{
    private Rectangle? _selectionBorder;

    public FrameworkElement? SelectedElement { get; private set; }

    public event Action<FrameworkElement?>? SelectionChanged;

    public void Select(FrameworkElement element)
    {
        SelectedElement = element;

        if (_selectionBorder is null)
        {
            _selectionBorder = new Rectangle
            {
                Stroke = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                StrokeThickness = 2,
                StrokeDashArray = [4, 2],
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };

            selectionOverlay.Children.Add(_selectionBorder);
        }

        Update();

        SelectionChanged?.Invoke(element);
    }

    public void Clear()
    {
        SelectedElement = null;
        _selectionBorder = null;
        selectionOverlay.Children.Clear();

        SelectionChanged?.Invoke(null);
    }

    public void Update()
    {
        if (SelectedElement is not { } element || _selectionBorder is null)
            return;

        _selectionBorder.Width = element.ActualWidth;
        _selectionBorder.Height = element.ActualHeight;

        var position = element.TransformToVisual(selectionOverlay).TransformPoint(new Point(0, 0));

        Canvas.SetLeft(_selectionBorder, position.X);
        Canvas.SetTop(_selectionBorder, position.Y);
    }

    public FrameworkElement? HitTest(Point position)
    {
        for (int i = document.SyntaxCount - 1; i >= 0; i--)
        {
            FrameworkElement element = document.ElementAt(i);

            if (ContainsPoint(element, position))
                return element;
        }

        return null;
    }

    private bool ContainsPoint(FrameworkElement element, Point point)
    {
        var position = element.TransformToVisual(designSurface).TransformPoint(new Point(0, 0));

        return point.X >= position.X &&
               point.X <= position.X + element.ActualWidth &&
               point.Y >= position.Y &&
               point.Y <= position.Y + element.ActualHeight;
    }
}
