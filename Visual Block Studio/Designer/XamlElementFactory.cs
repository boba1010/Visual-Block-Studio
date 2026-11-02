using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Designer;

public static class XamlElementFactory
{
    public static (FrameworkElement Element, XamlElementSyntax Syntax) CreateTextBox()
    {
        var element = new TextBox
        {
            Text = "TextBox",
            FontSize = 16,
        };

        var syntax = new XamlTextBoxSyntax
        {
            Text = "TextBox",
            FontSize = 16,
        };

        return (element, syntax);
    }

    public static (FrameworkElement Element, XamlElementSyntax Syntax) CreateButton()
    {
        var element = new Button
        {
            Content = "Button",
            Width = 120,
            Height = 40
        };

        var syntax = new XamlButtonSyntax
        {
            Content = "Button",
            Width = 120,
            Height = 40
        };

        return (element, syntax);
    }

    public static (FrameworkElement Element, XamlElementSyntax Syntax) CreateGrid()
    {
        var element = new Grid
        {
            Width = 200,
            Height = 120
        };

        var syntax = new XamlGridSyntax
        {
            Width = 200,
            Height = 120
        };

        return (element, syntax);
    }
}