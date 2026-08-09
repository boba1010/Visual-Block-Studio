namespace Visual_Block_Studio.Models;

public sealed class WindowBlock : XamlBlock
{
    public string BaseClass { get; set; } = null!;
    public string WindowName { get; set; } = null!;
    public string Namespace { get; set; } = null!;
    public string Title { get; set; } = null!;

    public override List<PropertyBlock> PropertyBlocks { get; set; } = [];

    private string Prop(string name) => name switch
    {
        "x:Class" => $"x:Class=\"{BaseClass}.{WindowName}\"",
        "xmlns" => "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
        "x" => "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
        "local" => $"xmlns:local=\"using:{Namespace}\"",
        "Title" => $"Title=\"{Title}\"",
        _ => PropertyBlocks.FirstOrDefault(p => p.Property.Name == name)?.Property.Value is { } v
            ? $"{name}=\"{v}\""
            : ""
    };

    protected override string ComputePrefix() =>
$@"<Window 
    {Prop("x:Class")}
    {Prop("xmlns")}
    {Prop("x")}
    {Prop("local")}
    {Prop("Title")}>";

    protected override string ComputeSuffix() => "</Window>";
}
