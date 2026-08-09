using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.DTOs;

public class RecentDto
{
    public string Name { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public RecentType Type { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
