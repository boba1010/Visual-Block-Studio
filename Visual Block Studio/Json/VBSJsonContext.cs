using System.Text.Json.Serialization;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.DTOs.Explorer;

namespace Visual_Block_Studio.Json;

[JsonSerializable(typeof(BlockDto))]
[JsonSerializable(typeof(XamlBlockDto))]
[JsonSerializable(typeof(WindowBlockDto))]
[JsonSerializable(typeof(PropertyBlockDto))]
[JsonSerializable(typeof(ButtonBlockDto))]
[JsonSerializable(typeof(VBSProjectDto))]
[JsonSerializable(typeof(ProjectItemDto))]
[JsonSerializable(typeof(SolutionDto))]
[JsonSerializable(typeof(List<RecentDto>))]
[JsonSerializable(typeof(RecentDto))]
[JsonSerializable(typeof(VBlockDocumentDto))]
public partial class VBSJsonContext : JsonSerializerContext
{
}
