namespace Visual_Block_Studio.DTOs.Explorer;

public class VBSProjectDto
{
    public string Name { get; set; } = null!;
    public string TargetArchitecture { get; set; } = null!;
    public uint MemoryLimitBytes { get; set; }

    public string OutputPath { get; set; } = null!;

    // For example 'Visual Block Studio.vbsproj'
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;

    // For example 'Visual Block Studio.vbsproj.g.cs'
    public string GeneratedFileName { get; set; } = null!;
    public string GeneratedFilePath { get; set; } = null!;

    public string CsProjFileName { get; set; } = null!;
    public string CsProjFilePath { get; set; } = null!;

    public List<ProjectItemDto> LayoutFiles { get; set; } = [];
}
