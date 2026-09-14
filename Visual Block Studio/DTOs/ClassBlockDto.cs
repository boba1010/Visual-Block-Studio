using Visual_Block_Studio.Collections;

namespace Visual_Block_Studio.DTOs;

public class ClassBlockDto : CodeBlockDto
{
    public string Name { get; set; } = null!;
    public List<CodeBlockDto> Members { get; set; } = [];
}
