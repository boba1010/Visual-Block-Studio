using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Visual_Block_Studio.Rendering;
using Visual_Block_Studio.ViewModels;

namespace Visual_Block_Studio.Views;

public sealed partial class BlocksPage : Page
{
    public BlocksViewModel ViewModel { get; }
    private VBlockRenderer? _renderer;

    public BlocksPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<BlocksViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string path)
            await ViewModel.LoadBlocksAsync(path);
    }

    private void BlockCanvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
    {
        _renderer = new VBlockRenderer(sender.Device);
    }

    private void BlockCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (_renderer is null)
            return;

        _renderer.Render(args.DrawingSession, ViewModel.Editor);
    }

    private void BlockCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var canvas = (CanvasControl)sender;
        var point = e.GetCurrentPoint(canvas).Position;

        var position = new Vector2(
            (float)canvas.ActualWidth - (float)point.X,
            (float)point.Y);

        ViewModel.Editor.PointerPressed(position);

        canvas.CapturePointer(e.Pointer);
        canvas.Invalidate();
    }

    private void BlockCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var canvas = (CanvasControl)sender;
        var point = e.GetCurrentPoint(canvas).Position;
        var position = new Vector2((float)canvas.ActualWidth - (float)point.X, (float)point.Y);

        if (ViewModel.Editor.PointerMoved(position))
            canvas.Invalidate();
    }

    private void BlockCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var canvas = (CanvasControl)sender;
        ViewModel.Editor.PointerReleased();
        canvas.ReleasePointerCapture(e.Pointer);
        canvas.Invalidate();
    }

    private void BlockCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var canvas = (CanvasControl)sender;
        var point = e.GetCurrentPoint(canvas);

        float factor = point.Properties.MouseWheelDelta > 0 ? 1.1f : 0.9f;

        var position = new Vector2((float)point.Position.X, (float)point.Position.Y);

        ViewModel.Editor.ZoomAt(position, factor);

        canvas.Invalidate();
    }
}