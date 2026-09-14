using System.Text.Json.Serialization;

namespace Visual_Block_Studio.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$blockType")]
[JsonDerivedType(typeof(WindowBlockDto), "Window")]
[JsonDerivedType(typeof(ButtonBlockDto), "Button")]
[JsonDerivedType(typeof(PropertyBlockDto), "Property")]
[JsonDerivedType(typeof(WindowCodeBlockDto), "WindowCode")]
public abstract class BlockDto
{
    public Vector2Dto Position { get; set; } = new(50, 50);
    public Vector2Dto Size { get; set; } = new(800, 600);
}
