using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.Models;

public class Recent
{
    public string Name { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public RecentType Type { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string FriendlyDate => UpdatedAt.ToString("g");

    public string GetRecentIcon(RecentType type)
    {
        return type switch
        {
            RecentType.Solution => "\uE737",    // Workspace frame icon
            RecentType.Project => "\uEA86",     // Plus/Layout project icon
            RecentType.Folder => "\uE8B7",      // Directory folder icon
            _ => "\uE71D"                       // Default fallback
        };
    }
}
