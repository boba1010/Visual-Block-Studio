using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Helpers;

public static class BlockTypes
{
    private static readonly Dictionary<string, Type> Types = new()
    {
        [nameof(WindowBlock)] = typeof(WindowBlock),
        [nameof(ButtonBlock)] = typeof(ButtonBlock),
    };

    public static Type Get(string name) => Types.TryGetValue(name, out var type) ? type : throw new InvalidOperationException(
        $"Unknown XAML block type: {name}");
}
