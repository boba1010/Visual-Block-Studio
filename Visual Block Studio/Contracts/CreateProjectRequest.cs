namespace Visual_Block_Studio.Contracts;

public class CreateProjectRequest
{
    public string ProjectName { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
}
