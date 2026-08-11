using System.Text.Json.Serialization;

namespace Visual_Block_Studio.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$blockType")]
[JsonDerivedType(typeof(WindowBlockDto), typeDiscriminator: "Window")]
[JsonDerivedType(typeof(ButtonBlockDto), typeDiscriminator: "Button")]
public abstract class XamlBlockDto : BlockDto
{
    public List<PropertyBlockDto> PropertyBlocks { get; set; } = [];

    public ParentXamlBlockDto? Parent { get; set; }

    public List<XamlBlockDto> Children { get; set; } = [];

    public abstract string Prefix { get; set; }

    public abstract string Suffix { get; set; }
}
