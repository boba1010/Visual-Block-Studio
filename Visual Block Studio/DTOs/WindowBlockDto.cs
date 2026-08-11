namespace Visual_Block_Studio.DTOs;

public class WindowBlockDto : XamlBlockDto
{
    private string _prefix;
    private string _suffix = "</Window>";

    public WindowBlockDto()
    {
        _prefix = $@"
<Window
    {Prop("x:Class")}
    {Prop("xmlns")}
    {Prop("x")}
    {Prop("local")}
    {Prop("Title")}>";
    }

    public string BaseClass { get; set; } = null!;
    public string WindowName { get; set; } = null!;
    public string Namespace { get; set; } = null!;
    public string Title { get; set; } = null!;

    public override string Prefix 
    { 
        set => _prefix = value;
        get => _prefix;
    }

    public override string Suffix 
    {
        set => _suffix = value;
        get => _suffix;
    }

    private string Prop(string name) => name switch
    {
        "x:Class" => $"x:Class=\"{BaseClass}.{WindowName}\"",
        "xmlns" => "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
        "x" => "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
        "local" => $"xmlns:local=\"using:{Namespace}\"",
        "Title" => $"Title=\"{Title}\"",
        _ => PropertyBlocks.FirstOrDefault(p => p.Property.Name == name)?.Property.Value is { } v ? $"{name}=\"{v}\"" : ""
    };
}
