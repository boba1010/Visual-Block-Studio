using Visual_Block_Studio.Models.CodeBlocks;
using Visual_Block_Studio.Models.XamlBlocks;

namespace Visual_Block_Studio.Helpers;

public static class BlockTypes
{
    private static readonly Dictionary<string, Type> Types = new()
    {
        [nameof(WindowBlock)] = typeof(WindowBlock),
        [nameof(ButtonBlock)] = typeof(ButtonBlock),
        [nameof(WindowCodeBlock)] = typeof(WindowCodeBlock),
        [nameof(NamespaceBlock)] = typeof(NamespaceBlock),
    };

    public static Type Get(string name) => Types.TryGetValue(name, out var type) ? type : throw new InvalidOperationException(
        $"Unknown block type: {name}");
}
