namespace Visual_Block_Studio.DTOs;

public abstract class CodeBlockDto : BlockDto
{
    public ParentCodeBlockDto? Parent { get; set; }
    public string Header { get; set; } = null!;
}
