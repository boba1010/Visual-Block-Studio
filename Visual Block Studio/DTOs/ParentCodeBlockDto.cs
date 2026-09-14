using Visual_Block_Studio.Models.CodeBlocks;

namespace Visual_Block_Studio.DTOs;

public sealed class ParentCodeBlockDto
{
    public string BlockType { get; init; } = null!;

    public static explicit operator ParentCodeBlockDto(ParentCodeBlock? v)
    {
        return new()
        {
            BlockType = v?.BlockType.Name
        };
    }
}
