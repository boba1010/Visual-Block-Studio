namespace Visual_Block_Studio.Models;

public class Recent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string FileContent { get; set; } = null!;
    public DateTimeOffset UpdatedAt { get; set; }
    public string FriendlyDate => UpdatedAt.ToString("g");
}
