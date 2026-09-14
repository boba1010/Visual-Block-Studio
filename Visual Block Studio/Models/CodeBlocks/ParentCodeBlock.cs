using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Helpers;

namespace Visual_Block_Studio.Models.CodeBlocks;

public sealed class ParentCodeBlock
{
    public Type BlockType { get; init; } = null!;

    public static explicit operator ParentCodeBlock(ParentCodeBlockDto? v)
    {
        return new()
        {
            BlockType = BlockTypes.Get(v?.BlockType)
        };
    }
}
