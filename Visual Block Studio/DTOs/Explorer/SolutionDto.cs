namespace Visual_Block_Studio.DTOs.Explorer;

public class SolutionDto
{
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string SlnxFileName { get; set; } = null!;
    public string SlnxFilePath { get; set; } = null!;
    public List<VBSProjectDto> Projects { get; set; } = [];
}
