using CommunityToolkit.Mvvm.Messaging;
using Visual_Block_Studio.Designer;
using Visual_Block_Studio.Designer.Properties;
using Visual_Block_Studio.Designer.Selection;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;
using Windows.ApplicationModel.DataTransfer;
using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Parsing;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;
using XamlPreviewer;

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
    private Recent _recent = null!;
    private readonly RecentsService _recents;
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
        _recents = App.Services.GetRequiredService<RecentsService>();

        _selection.SelectionChanged += element =>
        {
            if (element is null)
            {
                DeleteButton.IsEnabled = false;
                _propertyInspector.Clear();
                return;
            }

            DeleteButton.IsEnabled = true;
            _propertyInspector.Inspect(element);
        };

        DesignSurface.SizeChanged += (_, _) => _selection.Update();
        DesignSurface.PointerPressed += DesignSurface_PointerPressed;

        WeakReferenceMessenger.Default.Register<SaveMessage>(this, (r, m) => SaveChangesIfExists());

        WeakReferenceMessenger.Default.Register<SaveAllMessage>(this, (r, m) => SaveChangesIfExists());

        WeakReferenceMessenger.Default.Register<ClosingAllMessage>(this, (r, m) =>
        {
            var xaml = GenerateXaml();
            var dirty = _recent.FileContent != xaml;
            m.Reply(dirty);
        });

        WeakReferenceMessenger.Default.Register<ClosingTabMessage>(this, (r, m) =>
        {
            if (_recent.Id != m.Id)
                return;

            try
            {

                var xaml = GenerateXaml();
                if (_recent.FileContent == xaml)
                {
                    m.Reply(true);
                    return;
                }

                m.Reply(ShowDialogAsync());
            }
            catch (Exception)
            {
            }

            async Task<bool> ShowDialogAsync()
            {
                var dialog = new ContentDialog()
                {
                    Title = "Unsaved changes to the following",
                    Content = _recent.Name,
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Don't Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot,
                };

                var result = await dialog.ShowAsync();
                if (result is ContentDialogResult.Primary)
                {
                    SaveChanges();
                    return true;
                }
                else if (result is ContentDialogResult.Secondary)
                    return true;

                return false;
            }
        });
    }

    private void SaveChangesIfExists()
    {
        var xaml = GenerateXaml();
        if (_recent.FileContent == xaml)
            return;

        _recent.FileContent = xaml;
        _recents.Update(_recent);
    }

    private void SaveChanges()
    {
        string xaml = GenerateXaml();
        _recent.FileContent = xaml;
        _recents.Update(_recent);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Recent recent)
        {
            _recent = recent;
            if (string.IsNullOrEmpty(recent.FileContent))
                return;

            _document.RootSyntax = XamlParser.Parse(recent.FileContent);
            RefreshGrid();
        }
    }

    private void RefreshGrid()
    {
        switch (_document.RootSyntax)
        {
            case XamlPanelSyntax panel:
                if (panel is XamlGridSyntax grid)
                    AddGrid(grid);
                break;
            case XamlContentSyntax content:
                if (content is XamlButtonSyntax button)
                    AddButton(button);
                break;
            case XamlTextBlockSyntax text:
                AddTextBlock(text);
                break;
        }
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
        AddButton();
    }

    private void AddTextBlock_Click(object sender, RoutedEventArgs e)
    {
        AddTextBlock();
    }

    private void AddGrid_Click(object sender, RoutedEventArgs e)
    {
        AddGrid();
    }

    private void AddGrid(XamlGridSyntax? gridSyntax = null)
    {
        if (gridSyntax is null)
        {
            var (element, syntax) = XamlElementFactory.CreateGrid();
            AddControl(element, syntax);
            return;
        }

        var grid = XamlFactory.CreateGrid(gridSyntax);

        if (PreviewSurface.Children.Count == 0)
        {
            PreviewSurface.Children.Add(grid);
            _document.RootSyntax = gridSyntax;
        }
        else
        {
            (PreviewSurface.Children[0] as Grid)?.Children.Add(grid);
            (_document.RootSyntax as XamlPanelSyntax)?.Children.Add(gridSyntax);
        }

        _selection.Select(grid);

        RegisterTree(grid, gridSyntax);
    }

    private void RegisterTree(FrameworkElement element, XamlElementSyntax syntax)
    {
        RegisterControl(element, syntax);

        if (element is not Panel panel || syntax is not XamlPanelSyntax panelSyntax)
            return;

        for (int i = 0; i < panelSyntax.Children.Count; i++)
        {
            if (panel.Children[i] is FrameworkElement child)
                RegisterTree(child, panelSyntax.Children[i]);
        }
    }

    private void AddButton(XamlButtonSyntax? buttonSyntax = null)
    {
        if (buttonSyntax is null)
        {
            var (element, syntax) = XamlElementFactory.CreateButton();
            AddControl(element, syntax);
            return;
        }

        var btn = XamlFactory.CreateButton(buttonSyntax);
        AddControl(btn, buttonSyntax);
    }

    private void AddTextBlock(XamlTextBlockSyntax? textSyntax = null)
    {
        if (textSyntax is null)
        {
            var (element, syntax) = XamlElementFactory.CreateTextBlock();
            AddControl(element, syntax);
            return;
        }

        var text = XamlFactory.CreateTextBlock(textSyntax);
        AddControl(text, textSyntax);
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

        RegisterControl(element, syntax);

        _selection.Select(element);
    }

    private void RegisterControl(FrameworkElement element, XamlElementSyntax syntax)
    {
        _document.AddSyntaxRelation(element, syntax);
        element.SizeChanged += Control_SizeChanged;
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
        string xaml = GenerateXaml();

        var package = new DataPackage();
        package.SetText(xaml);

        Clipboard.SetContent(package);
    }

    private string GenerateXaml()
    {
        if (_document.RootSyntax is null)
            return "";

        var generator = new XamlCodeGenerator();
        string xaml = generator.Generate(_document.RootSyntax);

        return xaml;
    }

    private void Delete_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        RemoveControl();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        RemoveControl();
    }

    private void RemoveControl()
    {
        var selected = _selection.SelectedElement;

        if (selected is null)
            return;

        _document.RemoveSyntaxRelation(selected);
        
        PreviewSurface.Children.Remove(selected);

        _selection.Clear();
    }

    private void Escape_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        _selection.Clear();
    }
}