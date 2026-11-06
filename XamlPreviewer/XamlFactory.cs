#nullable enable

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlPreviewer;

public static class XamlFactory
{
    public static UIElement? Create(XamlElementSyntax element)
    {
        if (element is XamlPanelSyntax panel)
        {
            switch (panel)
            {
                case XamlGridSyntax gridSyntax:
                    return CreateGrid(gridSyntax);
            }
        }
        else if (element is XamlContentSyntax content)
        {
            switch (content)
            {
                case XamlButtonSyntax buttonSyntax:
                    return CreateButton(buttonSyntax);
            }
        }
        else if (element is XamlTextBlockSyntax text)
            return CreateTextBlock(text);

        return null;
    }

    public static Grid CreateGrid(XamlGridSyntax syntax)
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

    public static Button CreateButton(XamlButtonSyntax syntax)
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

    public static TextBlock CreateTextBlock(XamlTextBlockSyntax syntax)
    {
        var bgc = syntax.Foreground;

        var textBlock = new TextBlock
        {
            Width = syntax.Width,
            Height = syntax.Height,
            Foreground = new SolidColorBrush(ColorHelper.FromArgb(bgc.A, bgc.R, bgc.G, bgc.B)),
            FontSize = syntax.FontSize,
            HorizontalAlignment = (HorizontalAlignment)syntax.HorizontalAlignment,
            VerticalAlignment = (VerticalAlignment)syntax.VerticalAlignment,
            Text = syntax.Text
        };

        return textBlock;
    }

    public static UIElement? CreateElement(XamlElementSyntax syntax)
    {
        return syntax switch
        {
            XamlGridSyntax grid => CreateGrid(grid),
            XamlButtonSyntax button => CreateButton(button),
            XamlTextBlockSyntax textBlock => CreateTextBlock(textBlock),
            _ => throw new NotSupportedException($"Preview does not support {syntax.GetType().Name}.")
        };
    }
}
