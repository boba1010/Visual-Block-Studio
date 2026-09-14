namespace Visual_Block_Studio.Models.XamlBlocks;

public sealed class ButtonBlock : XamlBlock
{
    public string Content { get; set; } = "Click Me";
    public string Name { get; set; } = "MyButton";
    public override List<PropertyBlock> PropertyBlocks { get; set; } = [];

    protected override string ComputePrefix() => $"<Button x:Name=\"{Name}\" Content=\"{Content}\">";
    protected override string ComputeSuffix() => "</Button>";
}
