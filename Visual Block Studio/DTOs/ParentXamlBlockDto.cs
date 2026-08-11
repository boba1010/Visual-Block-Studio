namespace Visual_Block_Studio.DTOs;

public sealed class ParentXamlBlockDto
{
    public string BlockType { get; init; } = null!;
    public Vector2Dto Position { get; init; } = null!;
    public Vector2Dto Size { get; init; } = null!;
}
