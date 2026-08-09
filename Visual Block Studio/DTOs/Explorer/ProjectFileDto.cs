using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.DTOs.Explorer;

public class ProjectFileDto
{
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;

    public ProjectFileType Type { get; set; }
}
