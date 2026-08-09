using System.Text.Json;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Json;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Services;

public sealed class RecentsService
{
    private readonly string _filePath;

    public RecentsService()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Visual Block Studio");

        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "recents.json");
    }

    public void Update(Recent recent)
    {
        var recents = Get();
        int index = recents.FindIndex(x => string.Equals(x.FilePath, recent.FilePath, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return;
        recents[index] = recent;

        using var fs = File.Create(_filePath);
        JsonSerializer.Serialize(fs, recents.MapItemsToDtos(), VBSJsonContext.Default.ListRecentDto);
    }

    public void Save(Recent recent)
    {
        var recents = Get();

        recents.Add(recent);

        using var fs = File.Create(_filePath);
        JsonSerializer.Serialize(fs, recents.MapItemsToDtos(), VBSJsonContext.Default.ListRecentDto);
    }

    public List<Recent> Get()
    {
        if (!File.Exists(_filePath))
            return [];

        using var fs = File.OpenRead(_filePath);

        return JsonSerializer.Deserialize(fs, VBSJsonContext.Default.ListRecentDto)?.MapDtosToItems() ?? [];
    }
}
