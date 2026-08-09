using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.Models
{
    public class ToolboxItem
    {
        public string Name { get; set; } = null!;
        public Block Block { get; set; } = null!;
        public string Glyph { get; set; } = null!;
        public ToolbarItemType Type { get; set; }
    }
}
