#nullable enable

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlPreviewer;

public static class XamlPreviewer
{
    public static object? Load(XamlElementSyntax element)
    {
        if (element is XamlWindowSyntax windowSyntax)
        {
            Window window = new()
            {
                Title = windowSyntax.Title,
            };

            if (windowSyntax.Content is XamlPanelSyntax panel)
            {
                switch (panel)
                {
                    case XamlGridSyntax gridSyntax:
                        var grid = CreateGrid(gridSyntax);
                        window.Content = grid;
                        break;
                }
            }

            return window;
        }

        return null;
    }

    private static Grid CreateGrid(XamlGridSyntax syntax)
    {
        var bgc = syntax.Background;

        var grid = new Grid
        {
            Width = syntax.Width,
            Height = syntax.Height,
            Background = new SolidColorBrush(ColorHelper.FromArgb(bgc.A, bgc.R, bgc.G, bgc.B)),
            HorizontalAlignment = (HorizontalAlignment)syntax.HorizontalAlignment,
            VerticalAlignment = (VerticalAlignment)syntax.VerticalAlignment,
        };

        foreach (var child in syntax.Children)
        {
            if (CreateElement(child) is UIElement element)
                grid.Children.Add(element);
        }

        return grid;
    }

    private static Button CreateButton(XamlButtonSyntax syntax)
    {
        var bgc = syntax.Background;

        var button = new Button
        {
            Width = syntax.Width,
            Height = syntax.Height,
            Background = new SolidColorBrush(ColorHelper.FromArgb(bgc.A, bgc.R, bgc.G, bgc.B)),
            HorizontalAlignment = (HorizontalAlignment)syntax.HorizontalAlignment,
            VerticalAlignment = (VerticalAlignment)syntax.VerticalAlignment,
        };

        if (syntax.Content is XamlElementSyntax element)
            button.Content = CreateElement(element);
        else
            button.Content = syntax.Content;

        return button;
    }

    private static UIElement? CreateElement(XamlElementSyntax syntax)
    {
        return syntax switch
        {
            XamlGridSyntax grid => CreateGrid(grid),
            XamlButtonSyntax button => CreateButton(button),
            _ => throw new NotSupportedException($"Preview does not support {syntax.GetType().Name}.")
        };
    }
}
