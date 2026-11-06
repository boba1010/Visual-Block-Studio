using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public sealed class XamlPreviewController
{
    private Window? _previewWindow;

    public void Show(XamlControlSyntax rootSyntax)
    {
        Close();

        var content = XamlPreviewer.XamlFactory.Create(rootSyntax);

        if (content is not UIElement element)
            return;

        _previewWindow = new Window
        {
            Content = element
        };

        _previewWindow.Activate();
    }

    public void Close()
    {
        _previewWindow?.Close();
        _previewWindow = null;
    }
}