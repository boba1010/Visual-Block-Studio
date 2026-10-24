using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Syntax.Controls;

namespace VBS.Test;

internal class Program
{
    public static void Main()
    {
        var windowSyntax = new XamlWindowSyntax();
        windowSyntax.Namespaces.Add(new("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
        windowSyntax.Namespaces.Add(new("x", "http://schemas.microsoft.com/winfx/2006/xaml"));

        var grid = new XamlGridSyntax();

        grid.Children.Add(new XamlButtonSyntax()
        {
            Content = "Button",
            Width = 200,
            Height = 40
        });

        windowSyntax.Content = grid;

        XamlCodeGenerator generator = new();
        var xaml = generator.Generate(windowSyntax);

        Console.WriteLine(xaml);
    }
}
