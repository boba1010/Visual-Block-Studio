using System.Numerics;

namespace Visual_Block_Studio.Models;

public class ParentXamlBlock
{
    public Type BlockType { get; init; } = null!;
    public Vector2 Position { get; init; }
    public Vector2 Size { get; init; }
}
