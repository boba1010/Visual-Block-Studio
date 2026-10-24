using Microsoft.UI.Xaml.Shapes;
using XamlBlocksCompiler.Models;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Views;

public sealed partial class XamlVisualEditor : Page
{
    private FrameworkElement? _selectedElement;
    private Rectangle? _selectionBorder;
    private readonly Dictionary<FrameworkElement, XamlElementSyntax> _syntaxByElement = [];

    private XamlWindowSyntax WindowSyntax { get; } = new();

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

        WindowSyntax.Namespaces.Add(new("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
        WindowSyntax.Namespaces.Add(new("x", "http://schemas.microsoft.com/winfx/2006/xaml"));

        DesignSurface.SizeChanged += (_, _) => UpdateSelection();
        DesignSurface.PointerPressed += DesignSurface_PointerPressed;
    }

    private void UpdateMode()
    {
        bool editing = Mode == XamlEditorMode.Edit;

        PreviewSurface.IsHitTestVisible = !editing;
        DesignSurface.IsHitTestVisible = editing;

        if (!editing)
            ClearSelection();
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
        var element = new Button
        {
            Content = "Button",
            Width = 120,
            Height = 40
        };

        var syntax = new XamlButtonSyntax()
        {
            Width = element.Width,
            Height = element.Height,
            Content = "Button"
        };

        AddControl(element, syntax);
    }

    private void AddTextBlock_Click(object sender, RoutedEventArgs e)
    {
        //AddControl(new TextBlock
        //{
        //    Text = "TextBlock",
        //    FontSize = 18
        //});
    }

    private void AddGrid_Click(object sender, RoutedEventArgs e)
    {
        var element = new Grid
        {
            Width = 200,
            Height = 120,
        };

        var syntax = new XamlGridSyntax()
        {
            Width = element.Width,
            Height = element.Height,
        };

        AddControl(element, syntax);
    }

    private void AddControl(FrameworkElement element, XamlElementSyntax syntax)
    {
        element.SizeChanged += Control_SizeChanged;

        if (PreviewSurface.Children.Count == 0)
            PreviewSurface.Children.Add(element);
        else
        {
            if (_selectedElement is null)
                return;

            if (_selectedElement is Panel panel)
                panel.Children.Add(element);
        }
        _syntaxByElement.Add(element, syntax);
        if (WindowSyntax.Content is null)
            WindowSyntax.Content = syntax;
        else if (WindowSyntax.Content is XamlGridSyntax syntaxGrid)
            syntaxGrid.Children.Add(syntax);

        SelectControl(element);
    }

    private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ReferenceEquals(sender, _selectedElement))
            UpdateSelection();
    }

    private void SelectControl(FrameworkElement element)
    {
        _selectedElement = element;

        if (_selectionBorder is null)
        {
            _selectionBorder = new Rectangle
            {
                Stroke = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
                StrokeThickness = 2,
                StrokeDashArray = [4, 2],
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };

            SelectionOverlay.Children.Add(_selectionBorder);
        }

        UpdateSelection();
        ShowProperties(element);
    }

    private void ClearSelection()
    {
        _selectedElement = null;
        SelectionOverlay.Children.Clear();
        _selectionBorder = null;
        PropertiesContainer.Children.Clear();
    }

    private void ShowProperties(FrameworkElement element)
    {
        PropertiesContainer.Children.Clear();

        if (!_syntaxByElement.TryGetValue(element, out var syntax))
            return;

        if (syntax is not XamlControlSyntax controlSyntax)
            return;

        if (controlSyntax is XamlContentSyntax content)
        {
            AddStringProperty("Content", content.Name, value =>
            {
                content.Content = value;

                switch (element)
                {
                    case ContentControl contentControl:
                        contentControl.Content = value;
                        break;

                    case ContentPresenter contentPresenter:
                        contentPresenter.Content = value;
                        break;
                }

                UpdateSelection();
            });
        }   

        AddNumberProperty("Width", controlSyntax.Width, value =>
        {
            controlSyntax.Width = value;
            element.Width = value;
            UpdateSelection();
        });

        AddNumberProperty("Height", controlSyntax.Height, value =>
        {
            controlSyntax.Height = value;
            element.Height = value;
            UpdateSelection();
        });

        if (element is Panel or Border or Control)
        {
            AddColorProperty("Background", controlSyntax.Background, value =>
            {
                controlSyntax.Background = value;

                var brush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(value.A, value.R, value.G, value.B));

                switch (element)
                {
                    case Panel panel:
                        panel.Background = brush;
                        break;

                    case Border border:
                        border.Background = brush;
                        break;

                    case Control control:
                        control.Background = brush;
                        break;
                }

                UpdateSelection();
            });
        }

        AddAlignmentProperty("Vertical Alignment", controlSyntax.VerticalAlignment, value =>
        {
            controlSyntax.VerticalAlignment = value;
            element.VerticalAlignment = (VerticalAlignment)value;
            
            UpdateSelection();
        });
        AddAlignmentProperty("Horizontal Alignment", controlSyntax.HorizontalAlignment, value =>
        {
            controlSyntax.HorizontalAlignment = value;
            element.HorizontalAlignment = (HorizontalAlignment)value;

            UpdateSelection();
        });
    }

    private void AddStringProperty(string name, string? content, Action<string> onChanged)
    {
        PropertiesContainer.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13
        });

        var editor = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Text = content,
        };

        editor.SelectionChanged += (sender, args) =>
        {
            var textBox = (TextBox)sender;
            onChanged(textBox.Text);
        };

        PropertiesContainer.Children.Add(editor);
    }

    private void AddAlignmentProperty(string name, Alignment alignment, Action<Alignment> onChanged)
    {
        PropertiesContainer.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13
        });

        var editor = new ComboBox
        {
            ItemsSource = Enum.GetValues<Alignment>(),
            SelectedItem = alignment,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        editor.SelectionChanged += (sender, args) =>
        {
            var comboBox = (ComboBox)sender;

            var picked = (Alignment)comboBox.SelectedItem;
            onChanged(picked);
        };

        PropertiesContainer.Children.Add(editor);
    }

    private void AddColorProperty(string name, Color color, Action<Color> onChanged)
    {
        PropertiesContainer.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13
        });

        var editor = new ColorPicker
        {
            Color = Microsoft.UI.ColorHelper.FromArgb(color.A, color.R, color.G, color.B),
            IsAlphaEnabled = true,
        };

        editor.ColorChanged += (_, args) =>
        {
            var picked = args.NewColor;
            onChanged(Color.Create(picked.R, picked.G, picked.B, picked.A));
        };

        PropertiesContainer.Children.Add(editor);
    }

    private void AddNumberProperty(string name, double value, Action<double> onChanged)
    {
        PropertiesContainer.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13
        });

        var container = new Grid() { ColumnSpacing = 10 };
        container.ColumnDefinitions.Add(new() { Width = new(50) });
        container.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });

        var autoToggle = new ToggleSwitch
        {
            Header = "Auto",
            IsOn = double.IsNaN(value)
        };

        var editor = new NumberBox
        {
            Value = double.IsNaN(value) ? 0 : value,
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            IsEnabled = !double.IsNaN(value),
            VerticalAlignment = VerticalAlignment.Center
        };

        autoToggle.Toggled += (_, args) =>
        {
            editor.IsEnabled = !autoToggle.IsOn;
            onChanged(autoToggle.IsOn ? double.NaN : editor.Value);
        };

        editor.ValueChanged += (_, args) =>
        {
            if (!autoToggle.IsOn && double.IsFinite(args.NewValue))
                onChanged(args.NewValue);
        };

        Grid.SetColumn(autoToggle, 0);
        Grid.SetColumn(editor, 1);

        container.Children.Add(autoToggle);
        container.Children.Add(editor);

        PropertiesContainer.Children.Add(container);
    }

    private void UpdateSelection()
    {
        if (_selectedElement is not { } element || _selectionBorder is null)
            return;

        _selectionBorder.Width = element.ActualWidth;
        _selectionBorder.Height = element.ActualHeight;

        var position = element.TransformToVisual(SelectionOverlay).TransformPoint(new Point(0, 0));

        Canvas.SetLeft(_selectionBorder, position.X);
        Canvas.SetTop(_selectionBorder, position.Y);
    }

    private bool ContainsPoint(FrameworkElement element, Point point)
    {
        var position = element.TransformToVisual(DesignSurface).TransformPoint(new Point(0, 0));

        return point.X >= position.X &&
               point.X <= position.X + element.ActualWidth &&
               point.Y >= position.Y &&
               point.Y <= position.Y + element.ActualHeight;
    }

    private FrameworkElement? HitTestControl(Point position)
    {
        for (int i = _syntaxByElement.Count - 1; i >= 0; i--)
        {
            FrameworkElement element = _syntaxByElement.Keys.ElementAt(i);

            if (ContainsPoint(element, position))
                return element;
        }

        return null;
    }

    private void DesignSurface_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (Mode == XamlEditorMode.Interactive)
            return;

        Point position = e.GetCurrentPoint(DesignSurface).Position;
        FrameworkElement? element = HitTestControl(position);

        if (element is null)
        {
            ClearSelection();
            return;
        }

        SelectControl(element);
        e.Handled = true;
    }

    public enum XamlEditorMode
    {
        Edit,
        Interactive
    }

    private void FullScreenPreview_Click(object sender, RoutedEventArgs e)
    {
        Window xamlWindow = (Window)XamlPreviewer.XamlPreviewer.Load(WindowSyntax)!;
        xamlWindow?.Activate();
    }
}