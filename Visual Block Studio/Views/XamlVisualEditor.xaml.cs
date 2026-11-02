using Visual_Block_Studio.Designer;
using Visual_Block_Studio.Designer.Properties;
using Visual_Block_Studio.Designer.Selection;
using Windows.ApplicationModel.DataTransfer;
using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Views;

public enum XamlEditorMode
{
    Edit,
    Interactive
}

public sealed partial class XamlVisualEditor : Page
{
    private readonly XamlDesignerDocument _document;
    private readonly SelectionManager _selection;
    private readonly PropertyInspector _propertyInspector;
    private readonly XamlPreviewController _preview;
    private XamlEditorMode _mode = XamlEditorMode.Edit;

    public XamlEditorMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value)
                return;

            _mode = value;
            UpdateMode();
        }
    }

    public XamlVisualEditor()
    {
        InitializeComponent();

        _document = new();
        _selection = new(SelectionOverlay, DesignSurface, _document);
        _propertyInspector = new(PropertiesContainer, _document, _selection.Update);
        _preview = new();

        _selection.SelectionChanged += element =>
        {
            if (element is null)
            {
                _propertyInspector.Clear();
                return;
            }

            _propertyInspector.Inspect(element);
        };

        DesignSurface.SizeChanged += (_, _) => _selection.Update();
        DesignSurface.PointerPressed += DesignSurface_PointerPressed;
    }

    private void UpdateMode()
    {
        bool editing = Mode == XamlEditorMode.Edit;

        PreviewSurface.IsHitTestVisible = !editing;
        DesignSurface.IsHitTestVisible = editing;

        if (!editing)
            _selection.Clear();
    }

    private void InteractiveModeToggle_Changed(object sender, RoutedEventArgs e)
    {
        Mode = InteractiveModeToggle.IsChecked == true ? XamlEditorMode.Interactive : XamlEditorMode.Edit;
    }

    private void PropertiesToggle_Changed(object sender, RoutedEventArgs e)
    {
        bool? isChecked = PropertiesToggle.IsChecked;
        if (!isChecked.HasValue)
            return;

        PropertiesView?.Visibility = isChecked.Value ? Visibility.Visible : Visibility.Collapsed;

        if (DesignScroll is not null)
            Grid.SetColumnSpan(DesignScroll, isChecked.Value ? 1 : 2);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var (element, syntax) = XamlElementFactory.CreateButton();
        AddControl(element, syntax);
    }

    private void AddTextBlock_Click(object sender, RoutedEventArgs e)
    {
        var (element, syntax) = XamlElementFactory.CreateTextBlock();
        AddControl(element, syntax);
    }

    private void AddGrid_Click(object sender, RoutedEventArgs e)
    {
        var (element, syntax) = XamlElementFactory.CreateGrid();
        AddControl(element, syntax);
    }

    private void AddControl(FrameworkElement element, XamlElementSyntax syntax)
    {
        if (PreviewSurface.Children.Count == 0)
        {
            PreviewSurface.Children.Add(element);
            _document.RootSyntax = syntax;
        }
        else if (_selection.SelectedElement is Panel panel && _document.RootSyntax is XamlPanelSyntax panelSyntax)
        {
            panel.Children.Add(element);
            panelSyntax.Children.Add(syntax);
        }
        else
            return;

        _document.AddSyntaxRelation(element, syntax);

        element.SizeChanged += Control_SizeChanged;

        _selection.Select(element);
    }

    private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _selection.SelectedElement))
            _selection.Update();
    }

    private void DesignSurface_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (Mode == XamlEditorMode.Interactive)
            return;

        var position = e.GetCurrentPoint(DesignSurface).Position;
        var element = _selection.HitTest(position);

        if (element is null)
            _selection.Clear();
        else
            _selection.Select(element);

        e.Handled = true;
    }

    private void FullScreenPreview_Click(object sender, RoutedEventArgs e)
    {
        if (_document.RootSyntax is not { } rootSyntax)
            return;

        _preview.Show((XamlControlSyntax)rootSyntax);
    }

    private void GenerateXaml_Click(object sender, RoutedEventArgs e)
    {
        if (_document.RootSyntax is null)
            return;

        var generator = new XamlCodeGenerator();
        string xaml = generator.Generate(_document.RootSyntax);

        var package = new DataPackage();
        package.SetText(xaml);

        Clipboard.SetContent(package);
    }
}