using System.Text.Json.Serialization;
using Visual_Block_Studio.DTOs;

namespace Visual_Block_Studio.JSON;

[JsonSerializable(typeof(RecentDto))]
[JsonSerializable(typeof(List<RecentDto>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class VBSJsonContext : JsonSerializerContext
{
}
