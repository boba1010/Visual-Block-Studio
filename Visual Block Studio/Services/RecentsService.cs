using System.Text.Json;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Services;

public sealed class RecentsService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions Options = new() { WriteIndented = true };

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

        for (int i = 0; i < recents.Count; i++)
        {
            if (recents[i].FilePath == recent.FilePath)
            {
                recents[i] = recent;
                break;
            }
        }

        using var fs = File.Create(_filePath);
        JsonSerializer.Serialize(fs, recents.MapItemsToDtos(), Options);
    }

    public void Save(Recent recent)
    {
        var recents = Get();

        recents.Add(recent);

        using var fs = File.Create(_filePath);
        JsonSerializer.Serialize(fs, recents.MapItemsToDtos(), Options);
    }

    public List<Recent> Get()
    {
        if (!File.Exists(_filePath))
            return [];

        using var fs = File.OpenRead(_filePath);

        return JsonSerializer.Deserialize<List<RecentDto>>(fs)?.MapDtosToItems() ?? [];
    }
}
