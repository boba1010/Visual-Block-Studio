namespace Visual_Block_Studio.DTOs;

public sealed class ButtonBlockDto : XamlBlockDto
{
    private string _prefix;
    private string _suffix;

    public ButtonBlockDto()
    {
        _prefix = $"<Button x:Name=\"{Name}\" Content=\"{Content}\">";
        _suffix = "</Button>";
    }

    public string Content { get; set; } = "Click Me";
    public string Name { get; set; } = "MyButton";

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
}
