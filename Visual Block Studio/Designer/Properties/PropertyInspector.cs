using Microsoft.UI;
using XamlBlocksCompiler.Models;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer.Properties;

public sealed class PropertyInspector(Panel propsContainer, XamlDesignerDocument document, Action updateSelection)
{
    public void Inspect(FrameworkElement element)
    {
        propsContainer.Children.Clear();

        if (!document.TryGetSyntax(element, out var syntax))
            return;

        if (syntax is not XamlControlSyntax control)
            return;

        if (control is XamlContentSyntax content)
        {
            AddBackgroundProperty(element, control);
            AddContentProperty(element, content);
        }

        if (control is XamlTextBlockSyntax text)
        {
            AddTextProperty(element, text);
            AddForegroundProperty(element, text);
        }

        if (control is XamlPanelSyntax panel)
            AddBackgroundProperty(element, panel);

        AddSizeProperties(element, control);
        AddAlignmentProperties(element, control);
    }

    private void AddContentProperty(FrameworkElement element, XamlContentSyntax content)
    {
        AddStringProperty("Content", content.Content?.ToString(), value =>
        {
            content.Content = value;

            switch (element)
            {
                case ContentControl control:
                    control.Content = value;
                    break;

                case ContentPresenter presenter:
                    presenter.Content = value;
                    break;
            }
        });
    }

    private void AddTextProperty(FrameworkElement element, XamlTextBlockSyntax text)
    {
        AddStringProperty("Text", text.Text, value =>
        {
            text.Text = value;

            if (element is TextBlock textBlock)
                textBlock.Text = value;
        });
    }

    private void AddSizeProperties(FrameworkElement element, XamlControlSyntax syntax)
    {
        AddNumberProperty("Width", syntax.Width, value =>
        {
            OnPropertyChanged(() =>
            {
                syntax.Width = value;
                element.Width = value;
            });
        });

        AddNumberProperty("Height", syntax.Height, value =>
        {
            OnPropertyChanged(() =>
            {
                syntax.Height = value;
                element.Height = value;
            });
        });
    }

    private void AddForegroundProperty(FrameworkElement element, XamlTextBlockSyntax syntax)
    {
        AddColorProperty("Foreground", syntax.Foreground, value =>
        {
            OnPropertyChanged(() =>
            {
                syntax.Foreground = value;

                if (element is TextBlock text)
                    text.Foreground = CreateBrush(value);
            });
        });
    }

    private void AddBackgroundProperty(FrameworkElement element, XamlControlSyntax syntax)
    {
        switch (syntax)
        {
            case XamlContentSyntax content:
                AddColorProperty("Background", content.Background, value =>
                {
                    OnPropertyChanged(() =>
                    {
                        content.Background = value;

                        if (element is ContentControl control)
                            control.Background = CreateBrush(value);
                    });
                });
                break;

            case XamlPanelSyntax panel:
                AddColorProperty("Background", panel.Background, value =>
                {
                    OnPropertyChanged(() =>
                    {
                        panel.Background = value;

                        if (element is Panel control)
                            control.Background = CreateBrush(value);
                    });
                });
                break;
        }
    }

    private static SolidColorBrush CreateBrush(Color value) => new(ColorHelper.FromArgb(value.A, value.R, value.G, value.B));

    private void AddAlignmentProperties(FrameworkElement element, XamlControlSyntax syntax)
    {
        AddAlignmentProperty("Vertical Alignment", syntax.VerticalAlignment, value =>
        {
            OnPropertyChanged(() =>
            {
                syntax.VerticalAlignment = value;
                element.VerticalAlignment = (VerticalAlignment)value;
            });
        });

        AddAlignmentProperty("Horizontal Alignment", syntax.HorizontalAlignment, value =>
        {
            OnPropertyChanged(() =>
            {
                syntax.HorizontalAlignment = value;
                element.HorizontalAlignment = (HorizontalAlignment)value;
            });
        });
    }

    private void AddStringProperty(string name, string? content, Action<string> onChanged)
    {
        propsContainer.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 13
        });

        var editor = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Text = content,
        };

        editor.TextChanged += (sender, args) =>
        {
            var textBox = (TextBox)sender;
            onChanged(textBox.Text);
        };

        propsContainer.Children.Add(editor);
    }

    private void AddAlignmentProperty(string name, Alignment alignment, Action<Alignment> onChanged)
    {
        propsContainer.Children.Add(new TextBlock
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

        propsContainer.Children.Add(editor);
    }

    private void AddColorProperty(string name, Color color, Action<Color> onChanged)
    {
        propsContainer.Children.Add(new TextBlock
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

        propsContainer.Children.Add(editor);
    }

    private void AddNumberProperty(string name, double value, Action<double> onChanged)
    {
        propsContainer.Children.Add(new TextBlock
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

        propsContainer.Children.Add(container);
    }

    private void OnPropertyChanged(Action update)
    {
        update();
        updateSelection();
    }

    public void Clear()
    {
        propsContainer.Children.Clear();
    }
}
