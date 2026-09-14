namespace Visual_Block_Studio.Models.CodeBlocks;

public sealed class WindowCodeBlock : ClassBlock
{
    public WindowCodeBlock() : this("MainWindow")
    {
    }
    public WindowCodeBlock(string name)
    {
        Name = name;
        Header = $"public sealed partial class {Name} : Window";
    }
}
