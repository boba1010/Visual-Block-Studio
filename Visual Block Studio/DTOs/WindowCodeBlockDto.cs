namespace Visual_Block_Studio.DTOs;

public sealed class WindowCodeBlockDto : ClassBlockDto
{
    public WindowCodeBlockDto() : this("MainWindow")
    {
    }
    public WindowCodeBlockDto(string name)
    {
        Name = name;
        Header = $"public sealed partial class {Name} : Window";
    }
}
