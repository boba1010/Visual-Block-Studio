namespace Visual_Block_Studio.Models;

public sealed class XamlDesignFile(string fileName, string fileContent)
{
    public string FileName { get; set; } = fileName;
    public string FileContent { get; set; } = fileContent;
}
