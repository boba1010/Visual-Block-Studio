using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.DTOs.Explorer;

public class ProjectItemDto
{
    public string Name { get; set; } = null!;
    public ProjectItemType Type { get; set; }
    public List<ProjectFileDto> Files { get; set; } = [];
}
