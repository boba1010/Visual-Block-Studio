using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.Models.Explorer;

public class ProjectFile
{
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;

    public ProjectFileType Type { get; set; }
}
